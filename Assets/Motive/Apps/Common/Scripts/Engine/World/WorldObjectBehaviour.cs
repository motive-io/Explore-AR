// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using Motive.Unity.Models;

namespace Motive.Unity.World
{
    public class WorldObjectBehaviour : MonoBehaviour
    {
        public GameObject InteractibleObject;
        public GameObject WorldObject;
        public GameObject AnimationTarget;

        public UnityAsset Asset { get; set; }

        public virtual GameObject GetAnimationTarget()
        {
            if (AnimationTarget)
            {
                return AnimationTarget;
            }

            return WorldObject;
        }

        void Start()
        {
            if (!InteractibleObject)
            {
                InteractibleObject = gameObject;
            }

            if (!WorldObject)
            {
                WorldObject = gameObject;
            }
        }
    }

}