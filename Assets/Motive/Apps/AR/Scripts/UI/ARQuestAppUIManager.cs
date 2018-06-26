// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
using Motive.Core.Models;
using Motive.Unity.Apps;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Manages the UI state of an AR quest app, e.g. toggling between AR and map mode.
    /// </summary>
    /// <seealso cref="UIModeManager{Motive.Unity.UI.ARQuestAppUIManager}" />
    public class ARQuestAppUIManager : UIModeManager<ARQuestAppUIManager>
    {
        public Mode CurrentMode { get; private set; }

        public Text TourNameText;
        public GameObject[] ActivateWhenRunning;

        /// <summary>
        /// Fires when the quest updates.
        /// </summary>
        public UnityEvent QuestUpdated;

        /// <summary>
        /// Fires when the UI mode changes.
        /// </summary>
        public UnityEvent OnModeChanged;

        /// <summary>
        /// Returns the currently running quest.
        /// </summary>
        public ScriptDirectoryItem RunningQuest { get; private set; }

        public OptionsDialogPanel StopTourConfirm;

        public Mode InitialMode = Mode.Map;

        public enum Mode
        {
            Map,
            AR
        }

        protected override void Awake()
        {
            if (QuestUpdated == null)
            {
                QuestUpdated = new UnityEvent();
            }

            if (OnModeChanged == null)
            {
                OnModeChanged = new UnityEvent();
            }

            AppManager.Instance.Initialized += (object sender, System.EventArgs e) =>
            {
                UpdateCurrentTour();

                ScriptRunnerManager.Instance.Updated += (_s, args) =>
                {
                    UpdateCurrentTour();
                };
            };

            if (!MainCamera)
            {
                MainCamera = Camera.main;
            }

            base.Awake();
        }

        protected override void Start()
        {
            if (InitialMode == Mode.Map)
            {
                DeactivateMode(Mode.AR);
                ActivateMode(Mode.Map);
            }
            else
            {
                DeactivateMode(Mode.Map);
                ActivateMode(Mode.AR);
            }

            base.Start();
        }

        public void UpdateCurrentTour()
        {
            // should only be one
            var runningEpisodes = ScriptRunnerManager.Instance.GetRunningItems();

            RunningQuest = runningEpisodes != null ? runningEpisodes.FirstOrDefault() : null;

            bool running = (RunningQuest != null);

            if (TourNameText)
            {
                if (running)
                {
                    TourNameText.text = RunningQuest.Title.ToUpper();
                }
                else
                {
                    TourNameText.text = null;
                }
            }

            if (ActivateWhenRunning != null)
            {
                foreach (var obj in ActivateWhenRunning)
                {
                    if (obj)
                    {
                        obj.SetActive(running);
                    }
                }
            }

            if (QuestUpdated != null)
            {
                QuestUpdated.Invoke();
            }
        }

        /// <summary>
        /// Stops the current quest.
        /// </summary>
        public void StopCurrentQuest()
        {
            // should only be one
            var runningEpisodes = ScriptRunnerManager.Instance.GetRunningItems();
            if (runningEpisodes == null || runningEpisodes.Count() > 1) return;

            var dirItem = runningEpisodes.FirstOrDefault();
            if (dirItem == null) return;

            if (StopTourConfirm)
            {
                StopTourConfirm.Show(new string[] { "stop", "cancel" }, (opt) =>
                    {
                        if (opt == "stop")
                        {
                            ScriptRunnerManager.Instance.Stop(dirItem);
                        }
                    });
            }
            else
            {
                ScriptRunnerManager.Instance.Stop(dirItem);
            }
        }

        public void SetMode(Mode mode)
        {
            if (CurrentMode != mode)
            {
                DeactivateMode(CurrentMode);
                ActivateMode(mode);

                OnModeChanged.Invoke();
            }
        }

        /// <summary>
        /// Activates the specified UI mode.
        /// </summary>
        /// <param name="mode">The mode.</param>
        public void ActivateMode(Mode mode)
        {
            CurrentMode = mode;

            switch (mode)
            {
                case Mode.AR:
                    SetARActive(true);
                    break;
                case Mode.Map:
                    SetMapActive(true);
                    break;
            }
        }

        /// <summary>
        /// Deactivates the specified mode.
        /// </summary>
        /// <param name="mode">The mode.</param>
        public void DeactivateMode(Mode mode)
        {
            switch (mode)
            {
                case Mode.AR:
                    SetARActive(false);
                    break;
                case Mode.Map:
                    SetMapActive(false);
                    break;
            }
        }

        /// <summary>
        /// Sets AR mode.
        /// </summary>
        public override void SetARMode()
        {
            SetMode(Mode.AR);
        }

        /// <summary>
        /// Sets Map mode.
        /// </summary>
        public override void SetMapMode()
        {
            SetMode(Mode.Map);
        }
    }
}