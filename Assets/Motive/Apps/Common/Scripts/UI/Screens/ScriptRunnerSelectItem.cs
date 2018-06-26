// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Unity.Apps;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    public class ScriptRunnerSelectItemEventArgs : EventArgs
    {
        public ScriptDirectoryItem ScriptDirectoryItem { get; private set; }

        public ScriptRunnerSelectItemEventArgs(ScriptDirectoryItem _script)
        {
            ScriptDirectoryItem = _script;
        }
    }

    /// <summary>
    /// Represents a single script runner in a table of script runners.
    /// </summary>
    public class ScriptRunnerSelectItem : TextImageItem
    {
        public Button StartTour;
        public Text Description;
        public GameObject DurationBlock;
        public Text DurationText;

        public GameObject DisplayWhenActive;

        public event EventHandler<ScriptRunnerSelectItemEventArgs> Launch;
        public event EventHandler<ScriptRunnerSelectItemEventArgs> Stop;
        public event EventHandler<ScriptRunnerSelectItemEventArgs> Reset;

        protected ScriptDirectoryItem m_scriptDirectoryItem;

        public void Populate(ScriptDirectoryItem directoryItem)
        {
            m_scriptDirectoryItem = directoryItem;

            Text.text = directoryItem.Title;
            Description.text = directoryItem.Description;

            if (directoryItem.PublishingStatus == PublishingStatus.Draft)
            {
                Text.text += " (draft)";
            }
            else if (directoryItem.PublishingStatus == PublishingStatus.Development)
            {
                Text.text += " (in development)";
            }

            UpdateState();
        }

        public virtual void UpdateState()
        {
            var isRunning = false;
            try
            {
                isRunning = ScriptRunnerManager.Instance.IsRunning(m_scriptDirectoryItem);
            }
            catch (Exception)
            {
                // ignored
            }

            if (StartTour)
            {
                StartTour.gameObject.SetActive(!isRunning);
            }

            if (DisplayWhenActive)
            {
                DisplayWhenActive.SetActive(isRunning);
            }
        }

        public void DoLaunch()
        {
            if (Launch != null)
            {
                Launch(this, new ScriptRunnerSelectItemEventArgs(m_scriptDirectoryItem));
                UpdateState();
            }
        }

        public void DoStop()
        {
            if (Stop != null)
            {
                Stop(this, new ScriptRunnerSelectItemEventArgs(m_scriptDirectoryItem));
                UpdateState();
            }
        }

        public void DoReset()
        {
            if (Reset != null)
            {
                Reset(this, new ScriptRunnerSelectItemEventArgs(m_scriptDirectoryItem));
                UpdateState();
            }
        }
    }

}