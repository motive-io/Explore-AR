// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive;
using Motive.Core.Json;
using Motive.Core.Models;
using Motive.Core.Storage;
using Motive.Core.Utilities;
using Motive.Unity.Scripting;
using Motive.Unity.Storage;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Motive.Unity.Apps
{
    /// <summary>
    /// Manages "script runners". Can be run as a quest or tour launcher
    /// with only one runner at a time, or to handle episodic or DLC content
    /// where multiple runners may be active at once.
    /// </summary>
    public class ScriptRunnerManager : Singleton<ScriptRunnerManager>
    {
        class RunningDirectoryItem
        {
            public string ScriptDirectoryItemId { get; set; }
        }

        class ScriptRunnerState
        {
            public RunningDirectoryItem[] RunningQuests { get; set; }
        }

        /// <summary>
        /// Allow multiple script runners to run simultaneously.
        /// </summary>
        public bool AllowMultiple { get; set; }

        IStorageAgent m_storageAgent;
        internal List<ScriptDirectoryItem> m_runningItems;

        public event EventHandler Updated;

        public ScriptRunnerManager()
        {
            m_runningItems = new List<ScriptDirectoryItem>();

            m_storageAgent = StorageManager.GetGameStorageAgent("runningScripts.json");
        }

        public IEnumerable<ScriptDirectoryItem> GetRunningItems()
        {
            return m_runningItems ?? null;
        }

        public void StopAll(Action onComplete = null, bool reset = false)
        {
            if (m_runningItems.Count == 0)
            {
                if (onComplete != null)
                {
                    onComplete();
                }
            }
            else
            {
                var iter = new BatchProcessor(m_runningItems.Count, onComplete);

                foreach (var q in m_runningItems.ToArray())
                {
                    Stop(q, () => { iter++; }, reset);
                }
            }
        }

        /// <summary>
        /// Launches all running quests given a directory of quests.
        /// </summary>
        /// <param name="scriptDirectory"></param>
        public void LaunchRunningScripts(ScriptRunnerDirectory scriptDirectory)
        {
            Action run = () =>
                {
                    var state = JsonHelper.Deserialize<ScriptRunnerState>(m_storageAgent);

                    HashSet<string> running = null;

                    if (state != null && state.RunningQuests != null)
                    {
                        running = new HashSet<string>(state.RunningQuests.Select(r => r.ScriptDirectoryItemId));
                    }

                    var toRun = new List<ScriptDirectoryItem>();
                    var wasRunning = new List<ScriptDirectoryItem>();

                    foreach (var item in GetScriptRunners())
                    {
                        if ((running != null && running.Contains(item.Id)) || item.Autoplay)
                        {
                            toRun.Add(item);

                            if (!item.Autoplay)
                            {
                                wasRunning.Add(item);
                            }
                        }
                    }

                // Make sure autoplays don't crowd out any quest that
                // was running before if allow multiple is disabled.
                if (!AllowMultiple && wasRunning.Count > 0)
                    {
                        toRun = wasRunning;
                    }

                    foreach (var quest in toRun)
                    {
                        if (quest.ScriptReference != null)
                        {
                            Launch(quest);
                        }
                    }
                };

            StopAll(run);
        }

        /// <summary>
        /// Launches a new script directory item. If "OneAtATime" is set, this will cancel any
        /// running quests.
        /// </summary>
        /// <param name="episode"></param>
        public virtual void Launch(ScriptDirectoryItem episode)
        {
#if MOTIVE_IAP
            var purchaseState = EpisodePurchaseManager.Instance.GetEpisodeState(episode);

            if (purchaseState != EpisodeSelectState.AvailableToLaunch && 
                purchaseState != EpisodeSelectState.Running)
            {
                return;
            }
#endif

            // when an episode is launched, stop all other episodes
            if (!AllowMultiple)
            {
                foreach (var dItem in m_runningItems.ToList())
                {
                    if (dItem.ScriptReference.ObjectId != episode.ScriptReference.ObjectId)
                    {
                        Stop(dItem);
                    }
                }
            }

            if (episode.ScriptReference != null)
            {
                ScriptManager.Instance.LaunchScript(episode.ScriptReference.ObjectId, episode.Id, (success) =>
                    {
                        Stop(episode);
                    });
            }

            m_runningItems.Add(episode);

            SaveState();

            lock (m_runningItems)
            {
                if (m_runningItems.Count == 1)
                {
                    Platform.Instance.StartLiveSession(this);
                }
            }

            if (Updated != null) Updated(this, EventArgs.Empty);
        }

        internal bool HasState(ScriptDirectoryItem dirItem)
        {
            if (dirItem == null || dirItem.ScriptReference == null)
            {
                return false;
            }

            return ScriptManager.Instance.HasState(dirItem.ScriptReference.ObjectId, dirItem.Id);
        }

        public bool IsRunning(ScriptDirectoryItem directoryItem)
        {
            return (m_runningItems) != null && m_runningItems.Exists(i => i.Id == directoryItem.Id);
        }

        private void SaveState()
        {
            JsonHelper.Serialize(m_storageAgent,
                new ScriptRunnerState
                {
                    RunningQuests = m_runningItems.Select(e => new RunningDirectoryItem { ScriptDirectoryItemId = e.Id }).ToArray()
                });
        }

        /// <summary>
        /// Stops running a script, optionally resetting its state.
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="onComplete"></param>
        /// <param name="reset">True if the quest's state should be reset</param>
        public void Stop(ScriptDirectoryItem runner, Action onComplete = null, bool reset = false)
        {
            m_runningItems.Remove(runner);

            if (runner.ScriptReference != null)
            {
                ScriptManager.Instance.StopRunningScript(runner.ScriptReference.ObjectId, runner.Id, reset, onComplete);
            }
            else
            {
                if (onComplete != null)
                {
                    onComplete();
                }
            }

            SaveState();

            lock (m_runningItems)
            {
                if (m_runningItems.Count == 0)
                {
                    Platform.Instance.StopLiveSession(this);
                }
            }

            if (Updated != null) Updated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Resets a quest and all associated data. Does not reset any global data for this quest
        /// (inventory, wallet, etc.)
        /// </summary>
        /// <param name="runner"></param>
        public void Reset(ScriptDirectoryItem runner)
        {
            m_runningItems.Remove(runner);

            if (runner.ScriptReference != null)
            {
                ScriptManager.Instance.StopRunningScript(runner.ScriptReference.ObjectId, runner.Id, true, null);
            }

            lock (m_runningItems)
            {
                if (m_runningItems.Count == 0)
                {
                    Platform.Instance.StopLiveSession(this);
                }
            }

            SaveState();

            if (Updated != null) Updated(this, EventArgs.Empty);
        }


        public IEnumerable<ScriptDirectoryItem> GetScriptRunners(bool showDev = false, bool showDraft = false)
        {
            var _showDev = BuildSettings.IsDebug || showDev;
            var _showDraft = BuildSettings.IsDebug || showDraft;

            return
                ScriptRunnerDirectory.Instance.AllItems
                    .Where(item => item.PublishingStatus == PublishingStatus.Published ||
                           item.PublishingStatus == PublishingStatus.Development && _showDev ||
                           item.PublishingStatus == PublishingStatus.Draft && _showDraft);
        }
    }

}