// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Utilities;
using UnityEngine;

namespace Motive.UI.Framework
{
    /// <summary>
    /// Manages a camera on a gyroscope.
    /// </summary>
    public class CameraGyroscope : MonoBehaviour
    {
        public bool AutoStartGyro = true;

        public float DebugRotationSpeed = 1f;

        private float initialYAngle = 0f;
        private float appliedGyroYAngle = 0f;
        public float calibrationYAngle = 0f;
        public float hdgDiff = 0f;
        public float camHdg = 0f;
        public float camCompositeHdg = 0f;
        public float camTilt = 0f;

        public Vector3 globalFwd;
        public Vector3 camFwd;

        public Vector3 globalUp;
        public Vector3 camWorldUp;
        public Vector3 camWorldFwd;

        public Transform m_deviceSwivel;
        public Transform m_compassSwivel;
        public Transform m_world;

        bool m_calibrateCompass;

        void Awake()
        {
            m_deviceSwivel = transform.parent;
            m_compassSwivel = m_deviceSwivel.parent;
            m_world = m_compassSwivel.parent;
        }

        void Start()
        {
            if (AutoStartGyro)
            {
                StartGyro();
            }
        }

        void Update()
        {
            ApplyGyroRotation();
            ApplyCalibration();

            if (m_calibrateCompass)
            {
                SetHeading((float)Platform.Instance.SystemCompass.TrueHeading);
                m_calibrateCompass = false;
            }
        }

        public void CalibrateYAngle()
        {
            calibrationYAngle = appliedGyroYAngle - initialYAngle; // Offsets the y angle in case it wasn't 0 at edit time.
        }

        public void SetHeading(float heading)
        {
            //      (x,z)
            // |----*
            // |   / 
            // |  /
            // |a/ 
            // +---------
            // tan(a) = x / y

            globalFwd = transform.TransformPoint(Vector3.forward);
            globalUp = transform.TransformPoint(Vector3.up);

            var camFwd = m_compassSwivel.InverseTransformPoint(globalFwd);
            var camUp = m_compassSwivel.InverseTransformPoint(globalUp);

            var camFwdProject = new Vector3(camFwd.x, camFwd.z).normalized;
            var camUpProject = new Vector3(camUp.x, camUp.z);

            var cross = Vector3.Cross(camFwdProject, camUpProject);

            camHdg = Mathf.Atan2(camFwd.x, camFwd.z) * Mathf.Rad2Deg;

            camTilt = Mathf.Asin(cross.z) * Mathf.Rad2Deg;

            // Adjust heading based on tilt
            hdgDiff = MathHelper.GetDegreesInRange(heading - camHdg);

#if UNITY_ANDROID
            if (camFwd.y > 0)
            {
                hdgDiff += 180;
            }
#endif

            var delta = hdgDiff + camTilt;

            m_compassSwivel.localRotation = Quaternion.Euler(0,
                delta
                /*+ transform.localEulerAngles.z*/, 0);
        }

        public void StartGyro()
        {
            Input.gyro.enabled = true;

            initialYAngle = transform.eulerAngles.y;

            //#if !UNITY_EDITOR
#if UNITY_ANDROID
            // On Android, 0,0,0 is the device screen-side-up. Rotate
            // around the X axis by 90 to fix this.
            m_deviceSwivel.localRotation = Quaternion.Euler(90, 0, 0);
#endif
#if UNITY_IPHONE
		m_deviceSwivel.localRotation = Quaternion.Euler(90, 0, 0);
#endif
#if UNITY_EDITOR
            transform.rotation = Quaternion.Euler(Vector3.zero);
#endif
            //#endif

            enabled = true;

            ApplyGyroRotation();
        }

        public void StopGyro()
        {
            m_compassSwivel.localRotation = Quaternion.identity;
            m_deviceSwivel.localRotation = Quaternion.identity;

            enabled = false;
        }

        public void CalibrateCompass()
        {
            m_calibrateCompass = true;
        }

        void ApplyDeviceRotation()
        {
            //    WORLD (LH)
            //
            //     y ^     z
            //       |   /
            //       |  /
            //       | /
            //       +------> x
            //

            var gyroEuler = Input.gyro.attitude.eulerAngles;

#if UNITY_ANDROID
            //    DEVICE HELD UPRIGHT (RH)
            //
            //     y ^
            //       |
            //       |
            //       +------> x
            //      /
            //     /
            //  z L
            // 
            // Z is already flipped. Don't alter its rotation.
            // X, Y lie in same orientation. Reverse for RH -> LH.
            transform.localRotation = Quaternion.Euler(-gyroEuler.x, -gyroEuler.y, gyroEuler.z);
#else
		transform.localRotation = Quaternion.Euler(-gyroEuler.x, -gyroEuler.y, gyroEuler.z);
		//transform.Rotate(0f, 0f, 180f, Space.Self); //Swap "handedness" of quaternion from gyro.
		//transform.Rotate(90f, 180f, 0f, Space.World); //Rotate to make sense as a camera pointing out the back of your device.
#endif
        }

        void ApplyDebugRotation()
        {
            if (Input.GetKey(KeyCode.Semicolon))
            {
                transform.rotation = Quaternion.Euler(Vector3.zero);
            }
            else
            {
                float x = 0f, y = 0f, z = 0f;

                if (Input.GetKey(KeyCode.I) || Input.GetKey(KeyCode.E))
                {
                    x = 1;
                }
                else if (Input.GetKey(KeyCode.M) || Input.GetKey(KeyCode.R))
                {
                    x = -1;
                }

                if (Input.GetKey(KeyCode.K))
                {
                    y = 1;
                }
                else if (Input.GetKey(KeyCode.J))
                {
                    y = -1;
                }

                if (Input.GetKey(KeyCode.L))
                {
                    z = 1;
                }
                else if (Input.GetKey(KeyCode.H))
                {
                    z = -1;
                }

                var rotation = new Vector3(x, y, z) * DebugRotationSpeed * 60f * Time.deltaTime;

                transform.Rotate(rotation);
            }

            CalibrateCompass();
        }

        void ApplyGyroRotation()
        {
#if UNITY_EDITOR
            ApplyDebugRotation();
#else
		ApplyDeviceRotation();
#endif
            appliedGyroYAngle = transform.eulerAngles.y; // Save the angle around y axis for use in calibration.
        }

        void ApplyCalibration()
        {
            transform.Rotate(0f, -calibrationYAngle, 0f, Space.World); // Rotates y angle back however much it deviated when calibrationYAngle was saved.
        }
    }
}