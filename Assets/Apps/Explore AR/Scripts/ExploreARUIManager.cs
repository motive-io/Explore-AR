// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.AR;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.Apps
{
    public class ExploreARUIManager : UIModeManager<ExploreARUIManager>
    {
        public Mode CurrentMode { get; private set; }

        public Text TourNameText;
        public GameObject[] ActivateWhenRunning;

        public enum Mode
        {
            Map,
            AR
        }

        protected override void Awake()
        {
            AppManager.Instance.Initialized += (object sender, System.EventArgs e) =>
            {
                UpdateCurrentTour();

                ScriptRunnerManager.Instance.Updated += (_s, args) =>
                {
                    UpdateCurrentTour();
                };
            };

            base.Awake();
        }

        protected override void Start()
        {
            DeactivateMode(Mode.AR);
            ActivateMode(Mode.Map);

            base.Start();
        }

        public void UpdateCurrentTour()
        {
            // should only be one
            var runningEpisodes = ScriptRunnerManager.Instance.GetRunningItems();

            var item = runningEpisodes != null ? runningEpisodes.FirstOrDefault() : null;

            bool running = (item != null);

            if (TourNameText)
            {
                if (running)
                {
                    TourNameText.text = item.Title.ToUpper();
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
        }

        public void StopCurrentTour()
        {
            // should only be one
            var runningEpisodes = ScriptRunnerManager.Instance.GetRunningItems();
            if (runningEpisodes == null || runningEpisodes.Count() > 1) return;

            var dirItem = runningEpisodes.FirstOrDefault();
            if (dirItem == null) return;

            ScriptRunnerManager.Instance.Stop(dirItem);
        }

        public void SetMode(Mode mode)
        {
            if (CurrentMode != mode)
            {
                DeactivateMode(CurrentMode);
                ActivateMode(mode);
            }
        }

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

        public override void SetARMode()
        {
            SetMode(Mode.AR);
        }

        public override void SetMapMode()
        {
            SetMode(Mode.Map);
        }
    }
}