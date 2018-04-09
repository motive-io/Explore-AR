// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.AR.LocationServices
{
    [Serializable]
    [CreateAssetMenu(fileName = "Location", menuName = "Motive/Location", order = 1)]
    public class UnityLocation : ScriptableObject
    {
        public string Name;

        [SerializeField]
        public UnityCoordinates Coordinates;
    }
}
