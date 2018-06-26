// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Linq;
using UnityEngine;

namespace Motive.Unity.Utilities
{
    public class EnableOnPlatform : MonoBehaviour
    {
        public RuntimePlatform[] Platforms;

        void Awake()
        {
            if (Platforms != null && !Platforms.Contains(Application.platform))
            {
                gameObject.SetActive(false);
            }
        }
    }
}