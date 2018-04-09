// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Unity.AR;
using Motive.Unity.Globalization;
using Motive.Unity.Utilities;
using System;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Driver for AR catcher minigame.
    /// </summary>
    public class ARCatcherMinigameDriver : LocationMinigameDriver<ARCatcherMinigame>
    {
        LocationARWorldObject m_worldObject;

        public override bool ShowMapAnnotation
        {
            get { return !Minigame.HideMapAnnotation; }
        }

        private ILocationAugmentedOptions GetOptions()
        {
            var opts = Minigame.AROptions ?? LocationAugmentedOptions.GetLinearDistanceOptions(false, TaskDriver.Task.ActionRange);

            if (TaskDriver.Task.ActionRange != null && opts.VisibleRange == null)
            {
                opts.VisibleRange = TaskDriver.Task.ActionRange;
            }

            return opts;
        }

        /// <summary>
        /// Add the AR object to AR World.
        /// </summary>
        public override void Start()
        {
            var collectible = TaskDriver.FirstCollectible;

            if (TaskDriver.Task.Locations != null &&
                TaskDriver.Task.Locations.Length > 0 &&
                collectible != null)
            {
                if (collectible.AssetInstance != null &&
                    collectible.AssetInstance.Asset != null)
                {
                    m_worldObject = ARWorld.Instance.AddLocationARAsset(
                        TaskDriver.Task.Locations[0],
                        0, // elevation
                        collectible.AssetInstance,
                        GetOptions());
                }

                if (m_worldObject == null)
                {
                    m_worldObject = ARWorld.Instance.AddLocationARImage(
                        TaskDriver.Task.Locations[0],
                        0,
                        collectible.ImageUrl,
                        ARWorld.Instance.GetDefaultImageOptions(TaskDriver.Task.ActionRange));
                }

                if (m_worldObject != null)
                {
                    m_worldObject.Clicked += m_worldObject_Clicked;
                }
            };

            if (ARCatcherMinigameDriverDelegate.Instance)
            {
                ARCatcherMinigameDriverDelegate.Instance.ShowTask(TaskDriver, m_worldObject);
            }

            base.Start();
        }

        public override void SetFocus(bool focus)
        {
            if (ARCatcherMinigameDriverDelegate.Instance)
            {
                ARCatcherMinigameDriverDelegate.Instance.SetFocus(TaskDriver, m_worldObject, focus);
            }

            base.SetFocus(focus);
        }

        void m_worldObject_Clicked(object sender, EventArgs e)
        {
            if (ARCatcherMinigameDriverDelegate.Instance)
            {
                ARCatcherMinigameDriverDelegate.Instance.Complete(TaskDriver, m_worldObject);
            }

            TaskDriver.Complete(m_worldObject.Location);
        }

        public override void Stop()
        {
            if (m_worldObject != null)
            {
                m_worldObject.Clicked -= m_worldObject_Clicked;

                ThreadHelper.Instance.CallOnMainThread(() =>
                {
                    ARWorld.Instance.RemoveWorldObject(m_worldObject);
                });
            }
        }

        public ARCatcherMinigameDriver(LocationTaskDriver driver, ARCatcherMinigame minigame) : base(driver, minigame)
        {
        }

        public override void Action()
        {
            if (ARCatcherMinigameDriverDelegate.Instance)
            {
                ARCatcherMinigameDriverDelegate.Instance.Action(TaskDriver, m_worldObject);
            }
        }

        public override bool ShowActionButton
        {
            get
            {
                if (ARCatcherMinigameDriverDelegate.Instance)
                {
                    return ARCatcherMinigameDriverDelegate.Instance.ShowActionButton;
                }
                else
                {
                    return false;
                }
            }
        }

        public override string ActionButtonText
        {
            get
            {
                return Localize.GetLocalizedString("ARTask.InRange", "In Range");
            }
        }

        public override string OutOfRangeActionButtonText
        {
            get
            {
                return Localize.GetLocalizedString("ARTask.OutOfRange", "Out of Range");
            }
        }
    }

}