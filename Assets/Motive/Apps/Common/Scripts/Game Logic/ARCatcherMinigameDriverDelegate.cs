// Copyright (c) 2018 RocketChicken Interactive Inc.
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

        public void ShowTask(LocationTaskDriver driver, ARWorldObject worldObject)
        {
        }

        public void SetFocus(LocationTaskDriver driver, ARWorldObject worldObject, bool focus)
        {
            if (worldObject != null)
            {
                var task = driver.Task;

                if (focus)
                {
                    ARViewManager.Instance.SetGuide(new ARGuideData
                    {
                        Instructions = task.Title,
                        Range = task.ActionRange,
                        WorldObject = worldObject
                    });

                    var text = driver.IsTakeTask ?
                        Localize.GetLocalizedString("ARAnnotation.TapToTake", "Tap to Collect") :
                        (driver.IsGiveTask ? Localize.GetLocalizedString("ARAnnotation.TapToPut", "Tap to Put") : null);

                    ARAnnotationViewController.Instance.AddTapAnnotation(worldObject, text);
                }
                else
                {
                    ARAnnotationViewController.Instance.RemoveTapAnnotation(worldObject);
                }
            }
        }

        public void HideTask(LocationTaskDriver driver, ARWorldObject worldObject)
        {
            // TODO: This can obliterate another caller that set the guide and annotation. Best case
            // would be a stack to only show the most recent of any guide/stack.
            ARViewManager.Instance.SetGuide(null);

            ARAnnotationViewController.Instance.RemoveTapAnnotation(worldObject);
        }

        public void Action(LocationTaskDriver driver, ARWorldObject worldObject)
        {
            if (OnAction != null)
            {
                OnAction.Invoke();
            }
        }

        public void Complete(LocationTaskDriver driver, ARWorldObject worldObject)
        {
            ARViewManager.Instance.SetTaskComplete(driver);
        }
    }
}