// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Manages the state of location task action buttons on a panel.
    /// </summary>
    public class LocationTaskActionButtonHandlerComponent : TaskActionButtonHandlerComponent<LocationTaskDriver>
    {
        public GameObject[] ARActionObjects;

        public override void Populate(LocationTaskDriver obj)
        {
            if (obj.MinigameDriver != null &&
                obj.MinigameDriver is ARCatcherMinigameDriver)
            {
                ObjectHelper.SetObjectsActive(ARActionObjects, true);

                DisableAllObjects();
            }
            else
            {
                ObjectHelper.SetObjectsActive(ARActionObjects, false);

                base.Populate(obj);
            }
        }
    }
}