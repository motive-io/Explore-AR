// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using UnityEngine;

namespace Motive.Unity.Utilities
{
    public class SingletonPanel<T> : Panel where T : SingletonPanel<T>
    {
        static T sInstance = null;

        public static T Instance
        {
            get { return sInstance; }
        }

        protected override void Awake()
        {
            base.Awake();

            if (sInstance != null)
            {
                Debug.LogError("SingletonComponent.Awake: error " + name + " already initialized");
            }

            sInstance = (T)this;
        }
    }

    public class SingletonPanel<T, D> : Panel<D> where T : SingletonPanel<T, D>
    {
        static T sInstance = null;

        public static T Instance
        {
            get { return sInstance; }
        }

        protected override void Awake()
        {
            base.Awake();

            if (sInstance != null)
            {
                Debug.LogError("SingletonComponent.Awake: error " + name + " already initialized");
            }

            sInstance = (T)this;
        }
    }

}