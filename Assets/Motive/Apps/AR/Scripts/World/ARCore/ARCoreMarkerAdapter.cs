// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using GoogleARCore;
using Motive.AR.Models;
using Motive.Unity.Utilities;
using UnityEngine;

namespace Motive.Unity.AR
{
    public class ARCoreMarkerAdapter : ARMarkerAdapterBase<RawImageMarker>
    {
#if MOTIVE_ARCORE
        private struct ExternApi
        {
#if UNITY_EDITOR
            public const string ARCoreNativeApi = "instant_preview_unity_plugin";
            public const string ARPrestoApi = "instant_preview_unity_plugin";
#else
            public const string ARCoreNativeApi = "arcore_sdk_c";
            public const string ARPrestoApi = "arpresto_api";
#endif

#pragma warning disable 626
            [DllImport(ARPrestoApi)]
            public static extern void ArPresto_getSession(ref IntPtr sessionHandle);

            [DllImport(ARPrestoApi)]
            public static extern void ArCoreUnity_setArPrestoInitialized(Action onEarlyUpdate);

            [DllImport(ARCoreNativeApi)]
            public static extern void ArConfig_create(IntPtr session, out IntPtr out_config);

            [DllImport(ARCoreNativeApi)]
            public static extern void ArSession_getConfig(IntPtr sessionHandle, IntPtr configHandle);

            [DllImport(ARCoreNativeApi)]
            public static extern int ArSession_configure(IntPtr sessionHandle, IntPtr configHandle);

            [DllImport(ARCoreNativeApi)]
            public static extern void ArConfig_setAugmentedImageDatabase(IntPtr sessionHandle, IntPtr configHandle, IntPtr dbHandle);

            [DllImport(ARCoreNativeApi)]
            public static extern void ArConfig_getAugmentedImageDatabase(IntPtr sessionHandle, IntPtr configHandle, IntPtr dbHandle);

            [DllImport(ARCoreNativeApi)]
            public static extern void ArAugmentedImageDatabase_create(IntPtr sessionHandle, out IntPtr pDb);

            [DllImport(ARCoreNativeApi)]
            public static extern void ArAugmentedImageDatabase_destroy(IntPtr augmentedImageDatabaseHandle);

            [DllImport(ARCoreNativeApi)]
            public static extern int ArAugmentedImageDatabase_deserialize(IntPtr sessionHandle,
                 IntPtr rawBytes, Int64 rawBytesSize, ref IntPtr outAugmentedImageDatabaseHandle);

            [DllImport(ARCoreNativeApi)]
            public static extern int ArAugmentedImageDatabase_addImage(IntPtr sessionHandle, IntPtr pDb, byte[] image_name, byte[] image_grayscale_pixels, Int32 image_width_in_pixels, Int32 image_height_in_pixels, Int32 image_stride_in_pixels, out Int32 out_index);

            [DllImport(ARCoreNativeApi)]
            public static extern int ArAugmentedImageDatabase_addImageWithPhysicalSize(IntPtr sessionHandle, IntPtr pDb, byte[] image_name, byte[] image_grayscale_pixels, Int32 image_width_in_pixels, Int32 image_height_in_pixels, Int32 image_stride_in_pixels, float image_width_in_meters, out Int32 out_index);

            [DllImport(ARCoreNativeApi)]
            public static extern void ArAugmentedImageDatabase_getNumImages(IntPtr sessionHandle, IntPtr pDb, out Int32 out_num_images);
#pragma warning restore 626
        }

        static ARCoreMarkerAdapter g_instance;
        
        public ARCoreTrackedImage TrackedImagePrefab;

        IntPtr m_sessionHandle;
        IntPtr m_dbHandle;

        private List<AugmentedImage> m_tempAugmentedImages = new List<AugmentedImage>();
        private Dictionary<int, ARCoreTrackedImage> m_trackedObjects = new Dictionary<int, ARCoreTrackedImage>();
        private Dictionary<int, string> m_markerIdentifiers = new Dictionary<int, string>();
        
        protected override void Awake()
        {
            g_instance = this;

            base.Awake();
        }
        
        System.Collections.IEnumerator TryInitialize()
        {
            m_sessionHandle = IntPtr.Zero;

            while (m_sessionHandle == IntPtr.Zero)
            {
                ExternApi.ArPresto_getSession(ref m_sessionHandle);

                m_logger.Debug("Got session handle {0}", m_sessionHandle);

                if (m_sessionHandle == IntPtr.Zero)
                {
                    yield return null;
                }
            }

            //#if !UNITY_EDITOR
            ExternApi.ArAugmentedImageDatabase_create(m_sessionHandle, out m_dbHandle);

            m_logger.Debug("Created DB with handle {0}", m_dbHandle);
            //#endif
        }

        public override void Initialize()
        {
            ThreadHelper.Instance.StartCoroutine(TryInitialize());

            base.Initialize();
        }

        private void Update()
        {
            if (Session.Status != SessionStatus.Tracking)
            {
                return;
            }
            
            Session.GetTrackables<AugmentedImage>(m_tempAugmentedImages, TrackableQueryFilter.Updated);

            // Create visualizers and anchors for updated augmented images that are tracking and do not previously
            // have a visualizer. Remove visualizers for stopped images.
            foreach (var image in m_tempAugmentedImages)
            {
                //m_logger.Debug("Found updated augmented image {0}", image.Name);

                ARCoreTrackedImage trackedObj = null;
                string identifier = null;

                m_trackedObjects.TryGetValue(image.DatabaseIndex, out trackedObj);
                m_markerIdentifiers.TryGetValue(image.DatabaseIndex, out identifier);

                if (identifier == null)
                {
                    // This marker is not registered, ignore it
                    return;
                }

                if (image.TrackingState == TrackingState.Tracking && trackedObj == null)
                {
                    m_logger.Debug("Started tracking {0}", image.Name);

                    try
                    {
                        // Create an anchor to ensure that ARCore keeps tracking this augmented image.
                        var imgPose = new Pose(image.CenterPose.position + transform.position, image.CenterPose.rotation);

                        Anchor anchor = image.CreateAnchor(imgPose);

                        trackedObj = Instantiate(TrackedImagePrefab);
                        trackedObj.transform.SetParent(anchor.transform);

                        trackedObj.transform.localPosition = Vector3.zero;
                        trackedObj.transform.localRotation = Quaternion.identity;
                        trackedObj.transform.localScale = Vector3.one;

                        trackedObj.Anchor = anchor;
                        trackedObj.Image = image;

                        m_trackedObjects.Add(image.DatabaseIndex, trackedObj);

                        m_logger.Debug("Adding object for {0} at pos={1} scale={2}", image.Name, trackedObj.transform.position, anchor.transform.localScale);
                        
                        OnStartedTracking(identifier, trackedObj.gameObject);
                    }
                    catch (Exception x)
                    {
                        m_logger.Exception(x);
                    }
                }
                else if (image.TrackingState == TrackingState.Stopped && trackedObj != null)
                {
                    m_logger.Debug("Stopped tracking {0}", image.Name);

                    OnLostTracking(identifier, trackedObj.gameObject);

                    m_trackedObjects.Remove(image.DatabaseIndex);
                    GameObject.Destroy(trackedObj);
                }
            }
        }

        public override void Activate()
        {
            base.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }
        
        void UpdateConfig()
        {
            IntPtr cfgHandle = IntPtr.Zero;

            ExternApi.ArConfig_create(m_sessionHandle, out cfgHandle);

            ExternApi.ArSession_getConfig(m_sessionHandle, cfgHandle);

            m_logger.Debug("Got config handle {0}", cfgHandle);

            ExternApi.ArConfig_setAugmentedImageDatabase(m_sessionHandle, cfgHandle, m_dbHandle);

            var result = ExternApi.ArSession_configure(m_sessionHandle, cfgHandle);

            m_logger.Debug("Set session config with result={0}", result);
        }

        public override void RegisterMarker(RawImageMarker marker)
        {
            if (marker.TargetImage == null)
            {
                return;
            }

            string url = marker.TargetImage.Url;

            m_logger.Debug("Registering image target {0}", url);

            ImageLoader.LoadTexture(url, (tex) =>
            {
                var pxs = tex.GetPixels32();

                var gs = new byte[pxs.Length];

                m_logger.Debug("Got px len={0}", pxs.Length);

                for (int iy = 0, oy = pxs.Length - tex.width;
                    iy < pxs.Length;
                    iy += tex.width, oy -= tex.width)
                {
                    for (int x = 0; x < tex.width; x++)
                    {
                        int gidx = oy + x;
                        var c = pxs[iy + x];

                        gs[gidx] = (byte)((c.r + c.g + c.b) / 3);
                    }
                }

                //System.IO.File.WriteAllBytes(@"C:\Users\ryan\Projects\Sandbox\img", gs);

                //#if !UNITY_EDITOR
                int idx;

                m_logger.Debug("ArAugmentedImageDatabase_addImage({0}, {1}, {2}, {3})", marker.Name, tex.width, tex.height, tex.width);

                int result = 0;

                if (marker.ImageSize != null && marker.ImageSize.Width.HasValue)
                {
                    result = ExternApi.ArAugmentedImageDatabase_addImageWithPhysicalSize(
                        m_sessionHandle, m_dbHandle, Encoding.UTF8.GetBytes(marker.Name), gs, tex.width, tex.height, tex.width, (float)marker.ImageSize.Width.Value, out idx);
                }
                else
                {
                    result = ExternApi.ArAugmentedImageDatabase_addImage(
                        m_sessionHandle, m_dbHandle, Encoding.UTF8.GetBytes(marker.Name), gs, tex.width, tex.height, tex.width, out idx);
                }

                m_logger.Debug("Got result {0} from ArAugmentedImageDatabase_addImage idx={1}", result, idx);

                m_markerIdentifiers[idx] = marker.GetIdentifier();

                UpdateConfig();

                int numImages;

                ExternApi.ArAugmentedImageDatabase_getNumImages(m_sessionHandle, m_dbHandle, out numImages);

                m_logger.Debug("DB now has {0} images", numImages);

                //#endif
            });

            base.RegisterMarker(marker);
        }

#endif
    }
}
