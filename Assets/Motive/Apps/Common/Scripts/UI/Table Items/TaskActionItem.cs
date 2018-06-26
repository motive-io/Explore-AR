// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    public class TaskActionItem : MonoBehaviour
    {
        public RawImage Image;
        public Text ItemName;
        public Text Count;
        public GameObject SatisfiedObject;
        public string Id { get; set; }
    }

}