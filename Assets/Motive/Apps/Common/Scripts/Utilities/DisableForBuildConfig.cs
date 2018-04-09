// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Utilities;
using UnityEngine;

namespace Motive.Unity.Utilities
{
    /// <summary>
    /// Disables objects and components for the specified build config.
    /// </summary>
    public class DisableForBuildConfig : MonoBehaviour
    {
        public BuildConfig Config;
        public GameObject[] Objects;
        public MonoBehaviour[] Components;

        void Awake()
        {
            if (BuildSettings.Instance.BuildConfig == Config)
            {
                if (Objects != null)
                {
                    foreach (var obj in Objects)
                    {
                        if (obj)
                        {
                            obj.SetActive(false);
                        }
                    }
                }

                if (Components != null)
                {
                    foreach (var com in Components)
                    {
                        if (com)
                        {
                            com.enabled = false;
                        }
                    }
                }
            }
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}