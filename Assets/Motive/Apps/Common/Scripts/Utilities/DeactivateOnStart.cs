// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;

namespace Motive.Unity.Utilities
{
    public class DeactivateOnStart : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
            gameObject.SetActive(false);
        }
    }
}