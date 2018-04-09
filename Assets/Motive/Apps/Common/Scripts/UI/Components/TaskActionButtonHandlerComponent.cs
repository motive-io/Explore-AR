// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;
using UnityEngine;

namespace Motive.Unity.UI
{
    public class TaskActionButtonHandlerComponent : TaskActionButtonHandlerComponent<IPlayerTaskDriver>
    {
    }

    /// <summary>
    /// Shows/displays action buttons on a task info panel based on the task action.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TaskActionButtonHandlerComponent<T> : PanelComponent<T>
        where T : IPlayerTaskDriver
    {
        public GameObject[] TakeActionObjects;
        public GameObject[] PutActionObjects;
        public GameObject[] ConfirmActionObjects;
        public GameObject[] MinigameActionObjects;

        public override void Populate(T obj)
        {
            DisableAllObjects();

            var playerTask = obj.Task as PlayerTask;
            if (playerTask != null && playerTask.Minigame != null)
            {
                ObjectHelper.SetObjectsActive(MinigameActionObjects, true);
            }
            else if (obj.IsGiveTask)
            {
                ObjectHelper.SetObjectsActive(PutActionObjects, true);
            }
            else if (obj.IsTakeTask)
            {
                ObjectHelper.SetObjectsActive(TakeActionObjects, true);
            }
            else if (obj.IsConfirmTask)
            {
                ObjectHelper.SetObjectsActive(ConfirmActionObjects, true);
            }
        }

        public override void Populate(object obj)
        {
            base.Populate(obj);
        }

        public void DisableAllObjects()
        {
            ObjectHelper.SetObjectsActive(PutActionObjects, false);
            ObjectHelper.SetObjectsActive(TakeActionObjects, false);
            ObjectHelper.SetObjectsActive(ConfirmActionObjects, false);
            ObjectHelper.SetObjectsActive(MinigameActionObjects, false);
        }
    }

}