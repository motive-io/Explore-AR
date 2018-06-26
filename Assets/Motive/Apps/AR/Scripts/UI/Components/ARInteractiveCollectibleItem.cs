// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Represents a collectible item that you can use in AR mode.
    /// </summary>
    public class ARInteractiveCollectibleItem : CollectibleTableItem
    {
        public UnityEvent OnSuccess;
        public UnityEvent OnFailure;

        public void Success()
        {
            if (OnSuccess != null)
            {
                OnSuccess.Invoke();
            }
        }

        public void Fail()
        {
            if (OnFailure != null)
            {
                OnFailure.Invoke();
            }
        }
    }

}