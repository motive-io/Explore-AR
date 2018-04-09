// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.Apps
{
    /// <summary>
    /// Component that lets you manage app lifecycle that's
    /// safe for prefabs.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    /// <seealso cref="Motive.Unity.Apps.AppManager" />
    public class AppManagerControls : MonoBehaviour
    {
        /// <summary>
        /// Resets the app, deleting any saved game data.
        /// </summary>
        public void Reset()
        {
            AppManager.Instance.Reset();
        }

        /// <summary>
        /// Reloads reloads the app data from the Motive server without
        /// resetting.
        /// </summary>
        public void Reload()
        {
            AppManager.Instance.Reload();
        }
    }

}