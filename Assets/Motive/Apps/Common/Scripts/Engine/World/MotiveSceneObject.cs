// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;

namespace Motive.Unity.World
{
    /// <summary>
    /// Attach to a game object to make an object in your scene
    /// controllable by Motive.
    /// </summary>
    public class MotiveSceneObject : WorldObjectBehaviour
    {
        public string Name;

        // Use this for initialization
        void Start()
        {
            WorldObjectManager.Instance.RegisterNamedWorldObject(this);
        }
    }
}