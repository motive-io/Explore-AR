﻿// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.AR;
using Motive.Unity.Globalization;
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using UnityEngine.Events;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Delegate for AR catcher minigame driver.
    /// </summary>
    public class ARCatcherMinigameDriverDelegate : SingletonComponent<ARCatcherMinigameDriverDelegate>
    {
        public UnityEvent OnAction;
        public bool ShowActionButton;

        ARGuideData m_guideData;

        public void ShowTask(LocationTaskDriver driver, ARWorldObject worldObject)
        {
        }

        public void SetFocus(ARCatcherMinigameDriver minigameDriver, ARWorldObject worldObject, bool focus)
        {
            var driver = minigameDriver.TaskDriver;

            if (worldObject != null)
            {
                var task = driver.Task;

                if (focus)
                {
                    m_guideData = new ARGuideData
                    {
                        Instructions = task.Title,
                        Range = task.ActionRange,
                        WorldObject = worldObject
                    };

                    ARViewManager.Instance.SetGuide(m_guideData);

                    var text = driver.IsTakeTask ?
                        Localize.GetLocalizedString("ARAnnotation.TapToTake", "Tap to Collect") :
                        (driver.IsGiveTask ? Localize.GetLocalizedString("ARAnnotation.TapToPut", "Tap to Put") : null);

                    ARAnnotationViewController.Instance.AddTapAnnotation(worldObject, text, minigameDriver.Complete);
                }
                else
                {
                    if (m_guideData != null)
                    {
                        ARViewManager.Instance.ClearGuide(m_guideData);

                    }
                    ARAnnotationViewController.Instance.RemoveTapAnnotation(worldObject);
                }
            }
        }

        public void HideTask(ARCatcherMinigameDriver minigameDriver, ARWorldObject worldObject)
        {
            if (m_guideData != null)
            {
                ARViewManager.Instance.ClearGuide(m_guideData);
            }

            ARAnnotationViewController.Instance.RemoveTapAnnotation(worldObject);
        }

        public void Action(ARCatcherMinigameDriver driver, ARWorldObject worldObject)
        {
            if (OnAction != null)
            {
                OnAction.Invoke();
            }
        }

        public void Complete(ARCatcherMinigameDriver driver, ARWorldObject worldObject)
        {
            ARViewManager.Instance.SetTaskComplete(driver.TaskDriver);
        }
    }
}