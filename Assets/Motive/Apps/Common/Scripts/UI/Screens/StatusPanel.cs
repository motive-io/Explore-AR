// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Scripting;
using System.Text;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Debug panel that can display various Motive diagnostics.
    /// </summary>
    public class StatusPanel : Panel
    {
        public Text Text;

        public override void Populate()
        {
            var scripts = ScriptManager.Instance.GetRunningScripts();

            StringBuilder message = new StringBuilder();

            message.Append("Running scripts:\n\n");

            foreach (var script in scripts)
            {
                message.AppendFormat("{0} ({1})\n", script.Name, script.Id);
            }

            Text.text = message.ToString();
        }
    }

}