// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;

namespace Motive.Unity
{
    /// <summary>
    /// Represents a named links between objects in a scene.
    /// Useful to link between separate prefabs.
    /// </summary>
    public class ObjectLink : MonoBehaviour
    {
        public string ObjectName;
        public GameObject GameObject;

        void Awake()
        {
            if (!GameObject && !string.IsNullOrEmpty(ObjectName))
            {
                GameObject = GameObject.Find(ObjectName);
            }
        }

        public void Call(string method)
        {
            if (GameObject)
            {
                GameObject.SendMessage(method, SendMessageOptions.DontRequireReceiver);
            }
        }

        public void SetActive(bool active)
        {
            if (GameObject)
            {
                GameObject.SetActive(active);
            }
        }
    }

}