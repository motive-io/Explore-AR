// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Linq;
using UnityEngine;

namespace Motive.Unity.Utilities
{
    /// <summary>
    /// Disable the object this is attached to on certain platforms.
    /// </summary>
    public class DisableOnPlatform : MonoBehaviour
    {
        public RuntimePlatform[] Platforms;

        void Awake()
        {
            if (Platforms != null && Platforms.Contains(Application.platform))
            {
                gameObject.SetActive(false);
            }
        }
    } 
}
