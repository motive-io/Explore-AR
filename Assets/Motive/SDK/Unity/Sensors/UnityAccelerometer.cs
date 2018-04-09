// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Sensors;
using UnityEngine;

namespace Motive.Unity.Sensors
{
    /// <summary>
    /// Unity implementation of IAccelerometer.
    /// </summary>
    public class UnityAccelerometer : MonoBehaviour, IAccelerometer
    {
        public Vector Acceleration { get; private set; }

        void Update()
        {
            Acceleration = new Vector(Input.acceleration.x, Input.acceleration.y, Input.acceleration.z);
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}

