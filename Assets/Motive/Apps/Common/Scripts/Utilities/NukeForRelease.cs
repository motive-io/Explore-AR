// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;

namespace Motive.Unity.Utilities
{
    /// <summary>
    ///  Destroys the game object its attached to if running in release mode.
    /// </summary>
    public class NukeForRelease : MonoBehaviour
    {
        void Start()
        {
            if (BuildSettings.Instance.BuildConfig == BuildConfig.Release)
            {
                Destroy(gameObject);
            }
        }
    }

}