// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Models;
using Motive.Core.Scripting;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using Motive;
using Motive.Core.Utilities;
using Motive.Core.Globalization;
using Motive.Core.Storage;
using Motive.Core;
using Motive.Unity.Utilities;
using Motive.Unity.Storage;
using Motive.Unity.Playables;

namespace Motive.Unity.Scripting
{

    /// <summary>
    /// The Script Manager processes scripts downloaded from the server.
    /// This Script Manager uses a straightforward policy: execute any
    /// script named "Main."
    /// </summary>
    public class ScriptManager : SingletonComponent<ScriptManager>
    {
        Dictionary<string, ScriptProcessor> m_runningProcessors;
        Dictionary<string, Script> m_allScripts;
        SetDictionary<string, string> m_scriptFolders;
        IEnumerable<Script> m_scripts;

        public IEnumerable<Script> Scripts { get { return m_scripts; } }

        IStorageManager m_storageManager;

        const string ScriptManagerFolder = "scriptManager";

        public event EventHandler ScriptsReset;

        public bool AutoLaunchMains = true;

        public event EventHandler ScriptsUpdated;

        public string[] MainScriptNames = new string[] { "Main" };

        protected override void Awake()
        {
            base.Awake();

            m_runningProcessors = new Dictionary<string, ScriptProcessor>();
            m_scriptFolders = new SetDictionary<string, string>();
        }

        IStorageManager GetStorageManager()
        {
            if (Platform.Instance.UseEncryption)
                return m_storageManager ??
                       (m_storageManager = new EncryptedFileStorageManager(StorageManager.EnsureGameFolder(ScriptManagerFolder)));
            else
                return m_storageManager ??
                       (m_storageManager = new FileStorageManager(StorageManager.EnsureGameFolder(ScriptManagerFolder)));
        }

        public IEnumerable<Script> GetRunningScripts()
        {
            return m_runningProcessors.Values.Select(p => p.Script).ToArray();
        }

        void AddScriptFolder(string folderPath)
        {
            var parts = folderPath.Split('/');
            var path = "";

            foreach (var part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    m_scriptFolders.Add(string.IsNullOrEmpty(path) ? "/" : path, part);

                    path = path + "/" + part;
                }
            }
        }

        public IEnumerable<string> GetSubfolders(string path)
        {
            return m_scriptFolders[path];
        }

        public Script GetScript(string id)
        {
            if (m_allScripts == null)
            {
                return null;
            }

            Script script = null;

            m_allScripts.TryGetValue(id, out script);

            return script;
        }

        public void StopAllProcessors(Action onComplete)
        {
            var running = m_runningProcessors.Values.ToArray();
            m_runningProcessors.Clear();

            BatchProcessor.Process(running, (proc, onStop) =>
            {
                proc.StopRunning(onStop);
            }, onComplete);
        }

        internal void SetScripts(IEnumerable<Script> scripts)
        {
            m_scripts = scripts;

            if (m_scriptFolders != null)
            {
                m_scriptFolders.Clear();
            }
        }

        public void AbortAllProcessors()
        {
            var running = m_runningProcessors.Values.ToArray();
            m_runningProcessors.Clear();

            foreach (var proc in running)
            {
                proc.Abort();
            }
        }

        public void RunScripts()
        {
            if (m_scripts == null)
            {
                // We can support apps that don't have a script catalog now.
                //throw new ArgumentException("Must set catalog before calling RunScripts");
                return;
            }

            StorageManager.EnsureGameFolder(ScriptManagerFolder);

            m_allScripts = new Dictionary<string, Script>();

            List<Script> toRun = new List<Script>();

            string[] mains = null;

            if (MainScriptNames != null && MainScriptNames.Length > 0)
            {
                mains = MainScriptNames;
            }
            else
            {
                mains = new string[] { "main" };
            }

            foreach (var script in m_scripts)
            {
                m_allScripts[script.Id] = script;

                AddScriptFolder(script.Path);

                if (AutoLaunchMains && mains.Contains(script.Name, StringComparer.OrdinalIgnoreCase))
                {
                    toRun.Add(script);
                }
            }

            var callerMgr = GetStorageManager();

            Action startSoundtrack = () =>
            {
                if (AudioContentPlayer.Instance != null)
                {
                    AudioContentPlayer.Instance.StartSoundtrack();
                }
            };

            if (toRun.Count > 0)
            {
                // Start the soundtrack after all scripts have been launched
                // to make sure only the currently active soundtrack song gets
                // launched.
                BatchProcessor iter = new BatchProcessor(toRun.Count, startSoundtrack);

                foreach (var script in toRun)
                {
                    LaunchScript(script, null, callerMgr, () => { iter++; }, null);
                }
            }
            else
            {
                startSoundtrack();
            }

            if (ScriptsUpdated != null)
            {
                ScriptsUpdated(this, EventArgs.Empty);
            }
        }

        public void RunScriptCatalog(Catalog<Script> catalog)
        {
            StopAllProcessors(null);

            m_scripts = catalog;

            RunScripts();
        }

        internal void LaunchScript(ScriptLauncher launcher, FrameContext callingFrameContext, string runId, Action<bool> onComplete)
        {
            if (launcher == null ||
                launcher.ScriptReference == null ||
                launcher.ScriptReference.ObjectId == null)
            {
                if (onComplete != null)
                {
                    onComplete(false);
                }

                return;
            }

            var script = GetScript(launcher.ScriptReference.ObjectId);

            if (script == null)
            {
                if (onComplete != null)
                {
                    onComplete(false);
                }

                return;
            }

            var key = GetScriptRunId(script.Id, runId);

            if (m_runningProcessors.ContainsKey(key))
            {
                // Can only run one instance at a time with this implementation.
                // It is entirely possible to run the same script with multiple
                // script processors as long as they have different state files.
                if (onComplete != null)
                {
                    onComplete(false);
                }

                return;
            }

            // Store script state in its own directory so we can reset all
            // scripts easily.
            var folderName = callingFrameContext.ScriptContext.GetCompactId(launcher.Id);

            var stateMgr = callingFrameContext.ScriptContext.StorageManager.GetManager(folderName);

            var proc = new ScriptProcessor(launcher, callingFrameContext, script, stateMgr, runId);

            m_runningProcessors[key] = proc;

            proc.Run(null, onComplete);
        }

        string GetScriptFolderName(string scriptId)
        {
            return IdMapper.GetInternalLongId(scriptId).ToString();
        }

        public void LaunchScript(Script script, string runId, IStorageManager callerStorageManager, Action onRunning, Action<bool> onComplete)
        {
            AnalyticsHelper.FireEvent("LaunchScript", new Dictionary<string, object>
            {
                { "Name", script.Name },
                { "Id", script.Id },
                { "RunId", runId }
            });

            var key = GetScriptRunId(script.Id, runId);

            if (m_runningProcessors.ContainsKey(key))
            {
                // Can only run one instance at a time with this implementation.
                // It is entirely possible to run the same script with multiple
                // script processors as long as they have different state files.
                if (onComplete != null)
                {
                    onComplete(false);
                }

                return;
            }

            // Store script state in its own directory so we can reset all
            // scripts easily.
            var stateMgr = callerStorageManager.GetManager(GetScriptFolderName(script.Id), runId);

            var proc = new ScriptProcessor(script, stateMgr, runId);

            m_runningProcessors[key] = proc;

            proc.Run(onRunning, onComplete);
        }

        public void LaunchScriptsWithName(string scriptName, string runId = null, Action onRunning = null, Action<bool> onComplete = null)
        {
            var toRun = m_allScripts.Values.Where(s => s.Name == scriptName);

            foreach (var script in toRun)
            {
                LaunchScript(script, runId, onRunning, onComplete);
            }
        }

        public void LaunchScript(Script script, string runId, Action onRunning, Action<bool> onComplete)
        {
            LaunchScript(script, runId, GetStorageManager(), onRunning, onComplete);
        }

        public void Abort()
        {
            AbortAllProcessors();
        }

        public void Reset(Action onComplete)
        {
            AnalyticsHelper.FireEvent("Reset");

            StopAllProcessors(() =>
            {
                if (ScriptsReset != null)
                {
                    ScriptsReset(this, EventArgs.Empty);
                }

                ScriptEngine.Instance.Reset();

                StorageManager.DeleteGameFolder();

                if (onComplete != null)
                {
                    onComplete();
                }
            });
        }

        string GetScriptRunId(string scriptId, string runId)
        {
            return string.Format("{0}.{1}", scriptId, runId);
        }

        public void LaunchScript(string scriptId, string runId, Action<bool> onComplete)
        {
            if (m_allScripts.ContainsKey(scriptId))
            {
                LaunchScript(m_allScripts[scriptId], runId, GetStorageManager(), null, onComplete);
            }
            else
            {
                if (onComplete != null)
                {
                    onComplete(false);
                }
            }
        }

        // Use this for initialization
        protected override void Start()
        {
            base.Start();
        }

        public bool IsScriptRunning(Script script, string runId)
        {
            var key = GetScriptRunId(script.Id, runId);

            return (m_runningProcessors.ContainsKey(key));
        }

        public bool HasState(Script script, string runId)
        {
            return HasState(script.Id, runId);
        }

        public bool HasState(string scriptId, string runId)
        {
            return StorageManager.GameFolderExists(ScriptManagerFolder, GetScriptFolderName(scriptId), runId);
        }

        void DeleteScriptFolder(string scriptId, string runId)
        {
            StorageManager.DeleteGameFolder(ScriptManagerFolder, GetScriptFolderName(scriptId), runId);
        }

        public void StopRunningScript(string scriptId, string runId, bool resetState, Action onComplete)
        {
            var key = GetScriptRunId(scriptId, runId);

            ScriptProcessor proc = null;

            if (m_runningProcessors.TryGetValue(key, out proc))
            {
                proc.StopRunning(() =>
                {
                    m_runningProcessors.Remove(key);

                    if (resetState)
                    {
                        DeleteScriptFolder(scriptId, runId);
                    }

                    if (onComplete != null)
                    {
                        onComplete();
                    }
                });
            }
            else
            {
                if (resetState)
                {
                    DeleteScriptFolder(scriptId, runId);
                }

                if (onComplete != null)
                {
                    onComplete();
                }
            }
        }
    } 
}
