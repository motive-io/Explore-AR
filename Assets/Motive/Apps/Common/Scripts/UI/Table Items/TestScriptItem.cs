// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Unity.Scripting;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    public class TestScriptItemEventArgs : EventArgs
    {
        public Script Script { get; private set; }

        public TestScriptItemEventArgs(Script _script)
        {
            Script = _script;
        }
    }

    /// <summary>
    /// Used to display a script that can be launched directly by the user.
    /// </summary>
    public class TestScriptItem : MonoBehaviour
    {
        public Button LaunchButton;
        public Button StopButton;

        public Text ScriptName;
        public Text UpdatedOn;

        public event EventHandler<TestScriptItemEventArgs> Launch;
        public event EventHandler<TestScriptItemEventArgs> Stop;
        public event EventHandler<TestScriptItemEventArgs> Reset;

        Script m_script;

        public void Populate(Script script)
        {
            m_script = script;

            ScriptName.text = script.Name;
            UpdatedOn.text = script.LastModifiedTime.ToString();

            UpdateState();
        }

        public void UpdateState()
        {
            var isRunning = ScriptManager.Instance.IsScriptRunning(m_script, "test");

            LaunchButton.gameObject.SetActive(!isRunning);
            StopButton.gameObject.SetActive(isRunning);
        }

        public void DoLaunch()
        {
            if (Launch != null)
            {
                Launch(this, new TestScriptItemEventArgs(m_script));
            }
        }

        public void DoStop()
        {
            if (Stop != null)
            {
                Stop(this, new TestScriptItemEventArgs(m_script));
            }
        }

        public void DoReset()
        {
            if (Reset != null)
            {
                Reset(this, new TestScriptItemEventArgs(m_script));
            }
        }
    }

}