// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Simple map annotation handler that acts as a singleton.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="M"></typeparam>
    public class SingletonSimpleMapAnnotationHandler<T,M> :
        SimpleMapAnnotationHandler<M> 
            where T : SingletonSimpleMapAnnotationHandler<T,M> 
            where M : MapAnnotation
    {
        static T sInstance = null;

        public static T Instance
        {
            get { return sInstance; }
        }

        protected override void Awake()
        {
            if (sInstance != null)
            {
                Debug.LogError("SingletonMapAnnotationHandler.Awake: error " + name + " already initialized");
            }

            sInstance = (T)this;

            base.Awake();
        }
    }

    public class SingletonSimpleMapAnnotationHandler<T> : SingletonSimpleMapAnnotationHandler<T, MapAnnotation>
        where T : SingletonSimpleMapAnnotationHandler<T>
    {
    }
}