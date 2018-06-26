using Motive.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Motive.Unity.Utilities
{
    public static class TransformHelper
    {
        /// <summary>
        /// Computes the "forward" angle of the object transform in world transform space.
        /// </summary>
        /// <param name="objTransform"></param>
        /// <param name="worldTransform"></param>
        /// <returns></returns>
        public static float GetForwardAngle(Transform objTransform, Transform worldTransform)
        {
            // Get forward and up vectors in World space for the worldObj (which is usually
            // a camera)
            var globalCamFwd = objTransform.TransformPoint(Vector3.forward);

            // Compensate for the fact that the world might not be positioned at 0,0
            var delta = objTransform.position - worldTransform.position;

            // Map forward and up into world space
            var worldCamFwd = worldTransform.InverseTransformPoint(globalCamFwd - delta);
            
            var camHdg = Mathf.Atan2(worldCamFwd.x, worldCamFwd.z) * Mathf.Rad2Deg;

            return camHdg;
        }

        /// <summary>
        /// Gets the compass rotation to apply to the camera, taking into account how devices
        /// handle the "tilt" of the camera when computing true heading.
        /// </summary>
        /// <param name="cameraTransform"></param>
        /// <param name="worldTransform"></param>
        /// <param name="heading"></param>
        /// <returns></returns>
        public static float GetCompassRotation(Transform cameraTransform, Transform worldTransform, double heading)
        {
            // Get forward and up vectors in World space for the worldObj (which is usually
            // a camera)
            var globalCamFwd = cameraTransform.TransformPoint(Vector3.forward);
            var globalCamUp = cameraTransform.TransformPoint(Vector3.up);

            // Compensate for the fact that the world might not be positioned at 0,0
            var delta = cameraTransform.position - worldTransform.position;

            var worldCamFwd = worldTransform.InverseTransformPoint(globalCamFwd - delta);
            var worldCamUp = worldTransform.InverseTransformPoint(globalCamUp - delta);

            // Map forward and up into world space
            var camFwdProject = new Vector3(worldCamFwd.x, worldCamFwd.z).normalized;
            var camUpProject = new Vector3(worldCamUp.x, worldCamUp.z);

            // Compute the cross product of the forward and up projections. This
            // lets us compute the "tilt" of the device below
            var cross = Vector3.Cross(camFwdProject, camUpProject);

            var camHdg = Mathf.Atan2(worldCamFwd.x, worldCamFwd.z) * Mathf.Rad2Deg;

            var camTilt = Mathf.Asin(cross.z) * Mathf.Rad2Deg;

            // Adjust heading based on tilt
            var hdgDiff = MathHelper.GetDegreesInRange(camHdg - heading);

            //m_logger.Debug("hdgDiff={0} cross={1} tilt={2}", hdgDiff, cross, camTilt);

            var hdgDela = hdgDiff - camTilt;

            return (float)hdgDela;
        }
    }
}
