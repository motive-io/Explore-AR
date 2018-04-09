// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.UI.Framework;
using Motive.Unity.Scripting;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Debug panel that displays a list of scripts the user can launch.
    /// </summary>
    public class TestScriptsPanel : TablePanel
    {
        public TestScriptItem ItemPrefab;
        public TestScriptItem FolderPrefab;

        public bool ShowAll;
        public Toggle ShowAllToggle;
        public string ScriptPath;

        bool m_isInitialized;

        protected override void Awake()
        {
            base.Awake();

            if (ShowAllToggle)
            {
                ShowAllToggle.isOn = ShowAll;
            }

            m_isInitialized = true;
        }

        public override void Populate()
        {
            Table.Clear();

            var path = string.IsNullOrEmpty(ScriptPath) ? "/" : ScriptPath;

            var debug = ScriptManager.Instance.Scripts.ToList();

            IEnumerable<Script> scripts = ShowAll ?
                ScriptManager.Instance.Scripts : ScriptManager.Instance.Scripts.Where(s =>
                {
                    var _path = s.Path;
                    if (string.IsNullOrEmpty(_path)) _path = "/";
                    return _path == path;
                });

            if (!ShowAll && path != "/")
            {
                var parent = path.Substring(0, path.LastIndexOf("/"));

                var item = Table.AddItem(FolderPrefab);
                item.ScriptName.text = "..";

                item.Launch += (object sender, TestScriptItemEventArgs e) =>
                {
                    ScriptPath = parent;

                    Populate();
                };
            }

            foreach (var s in scripts.OrderBy(s => s.Name))
            {
                var item = Table.AddItem(ItemPrefab);
                item.Populate(s);

                item.Launch += (object sender, TestScriptItemEventArgs e) =>
                {
                    Close();

                    ScriptManager.Instance.LaunchScript(e.Script.Id, "test", null);
                };

                item.Reset += (object sender, TestScriptItemEventArgs e) =>
                {
                    ScriptManager.Instance.StopRunningScript(e.Script.Id, "test", true, () =>
                        {
                            item.UpdateState();
                        });
                };

                item.Stop += (object sender, TestScriptItemEventArgs e) =>
                {
                    ScriptManager.Instance.StopRunningScript(e.Script.Id, "test", false, () =>
                        {
                            item.UpdateState();
                        });
                };
            }

            if (!ShowAll)
            {
                var subfolders = ScriptManager.Instance.GetSubfolders(path);

                if (subfolders != null)
                {
                    foreach (var folder in subfolders.OrderBy(f => f))
                    {
                        var item = Table.AddItem(FolderPrefab);
                        item.ScriptName.text = folder;

                        item.Launch += (object sender, TestScriptItemEventArgs e) =>
                        {
                            ScriptPath = path + (path.EndsWith("/") ? "" : "/") + folder;

                            Populate();
                        };
                    }
                }
            }
        }

        public void ToggleShowAll()
        {
            if (!m_isInitialized) return;

            ShowAll = ShowAllToggle.isOn;

            if (isActiveAndEnabled)
            {
                Populate();
            }
        }
    }

}