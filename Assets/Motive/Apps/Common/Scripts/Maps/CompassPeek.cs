// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.Core.Utilities;
using UnityEngine;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Powers UI elements that point towards a compass heading up to a certain rotation.
    /// </summary>
    public class CompassPeek : MonoBehaviour
    {
        public Vector3 Axis = new Vector3(0, 0, 1);

        public float MaxPeek = 15;
        public float Dampening = 0.3f;

        double m_lastHeading;

        // Use this for initialization
        void Start()
        {
            m_lastHeading = ForegroundPositionService.Instance.Compass.TrueHeading;
        }

        // Update is called once per frame
        void Update()
        {
            var heading = ForegroundPositionService.Instance.Compass.TrueHeading;

            m_lastHeading = MathHelper.ApproachAngle(m_lastHeading, heading, Dampening, Time.deltaTime);

            var dh = m_lastHeading - heading;

            if (dh < -180)
            {
                dh += 360;
            }
            else if (dh > 180)
            {
                dh -= 360;
            }

            var deflection = Mathf.Clamp((float)(dh), -MaxPeek, MaxPeek);

            this.gameObject.transform.localRotation = Quaternion.AngleAxis(deflection, Axis);
        }
    }
}