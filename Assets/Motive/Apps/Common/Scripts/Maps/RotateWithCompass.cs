// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.Core.Utilities;
using UnityEngine;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Rotates a game object with the compass.
    /// </summary>
    public class RotateWithCompass : MonoBehaviour
    {
        /// <summary>
        /// Define the axis of rotation.
        /// </summary>
        public Vector3 Axis = new Vector3(0, 0, 1);
        /// <summary>
        /// Additional offset to apply to the rotation.
        /// </summary>
        public double Offset = 0;
        public bool UseMagneticHeading = false;
        /// <summary>
        /// If true, applies additional dampening to the rotation to reduce noise.
        /// </summary>
        public bool UseDampening;
        /// <summary>
        /// If true, rotate the object based on the relative bearing between the user and
        /// the target.
        /// </summary>
        public bool RelativeToUser;

        /// <summary>
        /// Optionally set a specific location for the object to point at.
        /// </summary>
        public UnityLocation PointAtLocation;

        private Coordinates m_pointAtCoords;

        // Use this for initialization
        void Start()
        {
            Input.compass.enabled = true;
            gameObject.transform.localRotation = Quaternion.AngleAxis(0f, Axis);

            if (PointAtLocation)
            {
                m_pointAtCoords = PointAtLocation.Coordinates.ToCoordinates();
            }
        }

        double GetCompassHeading()
        {
            return UseMagneticHeading ?
                ForegroundPositionService.Instance.Compass.MagneticHeading :
                ForegroundPositionService.Instance.Compass.TrueHeading;
        }

        double GetHeading()
        {
            if (m_pointAtCoords != null)
            {
                var bearing = ForegroundPositionService.Instance.Position.GetBearingTo(m_pointAtCoords);

                return RelativeToUser ? bearing : bearing - GetCompassHeading();
            }
            else
            {
                return GetCompassHeading();
            }
        }

        // Update is called once per frame
        void Update()
        {
            float targetAngle = (float)MathHelper.GetDegreesInRange(-(GetHeading() + Offset));

            if (UseDampening)
            {
                float currAngle;
                Vector3 currAxis;

                gameObject.transform.localRotation.ToAngleAxis(out currAngle, out currAxis);

                targetAngle = MathHelper.ApproachAngle(currAngle, targetAngle, 0.1f, Time.deltaTime);
            }

            this.gameObject.transform.localRotation = Quaternion.AngleAxis(targetAngle, Axis);
        }

        public void SetPointAtLocation(Location location)
        {
            if (location == null)
            {
                m_pointAtCoords = null;
                return;
            }

            m_pointAtCoords = location.Coordinates;
        }
    }

}