// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Motive.AR.Models;
using UnityEngine;
using UnityEngine.XR.iOS;

namespace Motive.Unity.AR
{
    public class ARKitMarkerAdapter : ARMarkerAdapterBase<RawImageMarker>
    {
        Dictionary<string, string> m_markerIdents = new Dictionary<string, string>();
        Dictionary<string, ARKitTrackedImage> m_trackedImages = new Dictionary<string, ARKitTrackedImage>();

        public ARKitTrackedImage TrackedImagePrefab;
        public ARKitWorldAdapter WorldAdapter;

#if MOTIVE_ARKIT
        public override void Initialize()
        {
            base.Initialize();

            UnityARSessionNativeInterface.ARImageAnchorAddedEvent += AddImageAnchor;
            UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent += UpdateImageAnchor;
            UnityARSessionNativeInterface.ARImageAnchorRemovedEvent += RemoveImageAnchor;

            if (!WorldAdapter)
            {
                WorldAdapter = GetComponent<ARKitWorldAdapter>();
            }
        }

        void AddImageAnchor(ARImageAnchor arImageAnchor)
        {
            try
            {
                if (arImageAnchor.referenceImageName != null)
                {
                    m_logger.Debug("image anchor added {0}", arImageAnchor.referenceImageName);

                    string ident;

                    if (m_markerIdents.TryGetValue(arImageAnchor.referenceImageName, out ident))
                    {
                        Vector3 position = UnityARMatrixOps.GetPosition(arImageAnchor.transform);
                        Quaternion rotation = UnityARMatrixOps.GetRotation(arImageAnchor.transform);

                        var trackedImage = Instantiate(TrackedImagePrefab, position, rotation);

                        trackedImage.ImageAnchor = arImageAnchor;
                        trackedImage.Identifier = ident;

                        trackedImage.transform.localScale = arImageAnchor.transform.lossyScale * arImageAnchor.referenceImagePhysicalSize;

                        m_logger.Debug("Setting pos={0} rot={1} scale={2}",
                                               trackedImage.gameObject.transform.position,
                                               trackedImage.gameObject.transform.rotation.eulerAngles,
                                               trackedImage.transform.localScale);

                        OnStartedTracking(ident, trackedImage.gameObject);

                        m_trackedImages[arImageAnchor.identifier] = trackedImage;
                    }
                }
            }
            catch (Exception x)
            {
                m_logger.Exception(x);
            }
        }

        void UpdateImageAnchor(ARImageAnchor arImageAnchor)
        {
            m_logger.Debug("image anchor updated {0}", arImageAnchor.referenceImageName);

            ARKitTrackedImage trackedImage;

            if (m_trackedImages.TryGetValue(arImageAnchor.identifier, out trackedImage))
            {
                trackedImage.gameObject.SetActive(true);

                trackedImage.gameObject.transform.position = UnityARMatrixOps.GetPosition(arImageAnchor.transform);
                trackedImage.gameObject.transform.rotation = UnityARMatrixOps.GetRotation(arImageAnchor.transform);

                trackedImage.transform.localScale = arImageAnchor.transform.lossyScale * arImageAnchor.referenceImagePhysicalSize;

                /*
                m_logger.Debug("Setting pos={0} rot={1} scale={2}", 
                               trackedImage.gameObject.transform.position,
                               trackedImage.gameObject.transform.rotation.eulerAngles,
                               trackedImage.transform.localScale);
                               */
            }
        }

        void RemoveImageAnchor(ARImageAnchor arImageAnchor)
        {
            m_logger.Debug("image anchor removed");

            ARKitTrackedImage trackedImage;

            if (m_trackedImages.TryGetValue(arImageAnchor.identifier, out trackedImage))
            {
                OnLostTracking(trackedImage.Identifier, trackedImage.gameObject);

                Destroy(trackedImage.gameObject);
            }
        }

        public override void RegisterMarker(RawImageMarker marker)
        {
            if (marker.TargetImage != null)
            {
                var imageUrl = WebServices.Instance.MediaDownloadManager.GetSystemUrl(marker.TargetImage.Url);

                double width = 0.2;

                if (marker.ImageSize != null)
                {
                    width = marker.ImageSize.Width.GetValueOrDefault(width);
                }

                WorldAdapter.AddTrackableImage(imageUrl, marker.Name, width);

                m_markerIdents[marker.Name] = marker.GetIdentifier();
            }

            base.RegisterMarker(marker);
        }

        public override void Deactivate()
        {
            // Disable all objects until we get an update
            foreach (var img in m_trackedImages.Values)
            {
                img.gameObject.SetActive(false);
            }

            base.Deactivate();
        }
#endif
    }
}