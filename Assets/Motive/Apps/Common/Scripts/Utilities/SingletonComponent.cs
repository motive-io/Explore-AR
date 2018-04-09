// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;

namespace Motive.Unity.Utilities
{
    public abstract class SingletonComponent<T> : MonoBehaviour where T : SingletonComponent<T>
    {
        static T sInstance = null;
        static string sComponentName;

        public static T Instance
        {
            get { return sInstance; }
        }

        protected virtual void Awake()
        {
            if (sInstance != null)
            {
                Debug.LogError("SingletonComponent.Awake: error initializing " + GetType().Name + " on " + name + ". (Already initialized on " + sComponentName + ")");
            }

            sComponentName = this.name;

            sInstance = (T)this;
        }

        protected virtual void Start()
        {

        }
    }
}