// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using Motive.AR.Models;
using UnityEngine;

using Logger = Motive.Core.Diagnostics.Logger;
using UnityEngine.Events;
using System.Collections;
using Motive.Unity.AR;

#if MOTIVE_VUFORIA
using Vuforia;

namespace Motive.AR.Vuforia
{
    public class VuforiaMarkerAdapter : ARMarkerAdapterBase<IVuforiaMarker>
    {
        public enum CameraRunningBehavior
        {
            OnlyWhileActive,
            AlwaysOn,
            AlwaysOnDelayedInit
        };

        public VuforiaBehaviour ARCamera;

        public UnityEvent Activated;
        public UnityEvent Deactivated;

        public bool IsInitialized { get; private set; }

        public CameraRunningBehavior CameraBehavior;

        bool m_doDeactivate;
        
        public Vector3 ItemScale = Vector3.one;

        public bool IsActive { get; private set; }

        HashSet<string> m_loadedDatabases;
        HashSet<MarkerDatabase> m_toLoad;

        Action m_onStart;
        
        public static VuforiaMarkerAdapter Instance { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            Instance = this;
            
            m_loadedDatabases = new HashSet<string>();
            m_toLoad = new HashSet<MarkerDatabase>();

            VuforiaARController.Instance.RegisterVuforiaStartedCallback(VuforiaStarted);

            if (Activated == null) Activated = new UnityEvent();
            if (Deactivated == null) Deactivated = new UnityEvent();

            // Disable the ARCamera so that the AR controller doesn't start Vuforia on us
            ARCamera.enabled = false;

            // Need to review: the code below errors out when trying to destroy
            // the trackables.
            // AppManager.Instance.Reloading += App_Reloading;
        }

        public override void Initialize()
        {
            if (CameraBehavior == CameraRunningBehavior.AlwaysOn)
            {
                StartCoroutine(InitVuforia(() =>
                {
                    CameraDevice.Instance.Start();

                    CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);

                    SetCameraActive(true);
                }));
            }
            else
            {
                SetCameraActive(false);
            }
        }

        private void SetCameraActive(bool isActive)
        {
            ARCamera.enabled = isActive;
            WorldCamera.enabled = isActive;
        }

        private void App_Reloading(object sender, EventArgs e)
        {
            // Reset all loaded databases
            m_loadedDatabases.Clear();
            m_toLoad.Clear();

            if (IsInitialized)
            {
                ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();

                foreach (var ds in objectTracker.GetActiveDataSets())
                {
                    ds.DestroyAllTrackables(true);
                }

                objectTracker.DestroyAllDataSets(true);
            }
        }

        void VuforiaStarted()
        {
            m_logger.Debug("VuforiaStarted: IsActive={0}, HasStarted={1}", IsActive, VuforiaARController.Instance.HasStarted);

            IsInitialized = true;

            if (!IsActive)
            {
                m_doDeactivate = true;
            }

            CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);

            //LoadDataSets();

            var onStart = m_onStart;
            m_onStart = null;

            if (onStart != null)
            {
                onStart();
            }
        }

        void LoadDataSets()
        {
            if (m_toLoad.Count > 0)
            {
                var toLoad = m_toLoad.ToArray();
                m_toLoad.Clear();

                foreach (var db in toLoad)
                {
                    LoadDataSet(db);
                }
            }
        }

        bool m_isInitializing;

        IEnumerator InitVuforia(Action onReady)
        {
            m_logger.Debug("InitVuforia");

            //m_onStart = onReady;

            //VuforiaARController.Instance.RegisterVuforiaInitializedCallback();

            if (!m_isInitializing)
            {
                m_isInitializing = true;

                // Enable the behaviour here because it needs to be enabled in order
                // to complete the initialization.
                ARCamera.enabled = true;

                VuforiaRuntime.Instance.InitVuforia();

                m_isInitializing = false;
            }

#if UNITY_2018_OR_LATER
            while (VuforiaRuntime.Instance.InitializationState != VuforiaRuntime.InitState.INITIALIZED)
#else
            while (!VuforiaRuntime.Instance.HasInitialized)
#endif
            {
                yield return 0;
            }

            onReady();
        }

        IEnumerator StartCamera(Action onReady)
        {
            SetCameraActive(true);

            //yield return 0;

            //Camera.SetActive(true);

            CameraDevice.Instance.Start();

            CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);

            //bool isReady = false;
            //      int updateCount = 0;

            //Action updated = () =>
            //{
            //          if (updateCount++ > 2)
            //          {
            //              isReady = true;
            //          }
            //};

            //VuforiaARController.Instance.RegisterTrackablesUpdatedCallback(updated);

            //while (!isReady)
            //      {
            //          VuforiaARController.Instance.UpdateState(true, true);

            //          yield return 0;
            //}

            //VuforiaARController.Instance.UnregisterTrackablesUpdatedCallback(updated);

            VuforiaARController.Instance.UpdateState(true, true);

            IsActive = true;

            yield return new WaitForEndOfFrame();

            onReady();
        }

        public override void Activate()
        {
            m_logger.Debug("Activate IsInitialized={0}", IsInitialized);

            Action onReady = () =>
            {
                m_logger.Debug("Vuforia initialized");

                IsActive = true;

                LoadDataSets();
            };

            Action start = () =>
            {
                StartCoroutine(StartCamera(onReady));
            };

            if (!IsInitialized)
            {
                // If camera is always on, skip the "start" call and
                // jump right to onReady. (AlwaysOn will handle the 
                // initialization).
                Action onInit = (CameraBehavior == CameraRunningBehavior.AlwaysOn) ?
                    onReady : start;

                StartCoroutine(InitVuforia(onInit));
            }
            else if (CameraBehavior == CameraRunningBehavior.OnlyWhileActive)
            {
                start();
            }
            else
            {
                SetCameraActive(true);

                onReady();
            }
        }

        public override void Deactivate()
        {
            IsActive = false;

            m_logger.Debug("Deactivate: IsInitialized = {0}", IsInitialized);

            if (IsInitialized)
            {
                m_doDeactivate = true;

                /*
                var handlers = m_trackingVuMarkers.Values.ToArray();

                foreach (var handler in handlers)
                {
                    StopTracking(handler);
                    m_trackingHold.Add(handler);
                }*/
            }

            if (Deactivated != null)
            {
                Deactivated.Invoke();
            }

            //TrackingConditionMonitor.TrackingMarkersUpdated();
        }

        void LateUpdate()
        {
            // Try to work around some Vuforia timing issues:
            // other components on this object may do work during this
            // update, so push off deactivating it until LateUpdate.
            if (m_doDeactivate)
            {
                m_logger.Debug("Deactivating now");

                m_doDeactivate = false;

                //gameObject.SetActive(false);            
                //Camera.SetActive(false);

                if (CameraBehavior == CameraRunningBehavior.OnlyWhileActive)
                {
                    CameraDevice.Instance.Stop();

                    SetCameraActive(false);
                }
            }
        }
        
        void LoadDataSet(MarkerDatabase database)
        {
            if (m_loadedDatabases.Contains(database.Id))
            {
                m_logger.Debug("Loading {0} - already loaded!", database.Id);

                return;
            }

            if (!VuforiaARController.Instance.HasStarted || !IsActive)
            {
                m_logger.Debug("Loading {0} - delaying load", database.Id);

                m_toLoad.Add(database);
                return;
            }

            m_logger.Debug("Loading {0}", database.Id);

            ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();

            DataSet dataSet = objectTracker.CreateDataSet();

            var fileName = WebServices.Instance.MediaDownloadManager.GetSystemUrl(database.XmlFile.Url);

            m_logger.Debug("Loading {0} from {1}", database.Id, fileName);

            if (dataSet.Load(fileName, VuforiaUnity.StorageType.STORAGE_ABSOLUTE))
            {
                objectTracker.Stop();  // stop tracker so that we can add new dataset

                if (!objectTracker.ActivateDataSet(dataSet))
                {
                    // Note: ImageTracker cannot have more than 100 total targets activated
                    Debug.Log("<color=yellow>Failed to Activate DataSet: " + fileName + "</color>");
                }

                if (!objectTracker.Start())
                {
                    Debug.Log("<color=yellow>Tracker Failed to Start.</color>");
                }

                m_loadedDatabases.Add(database.Id);

                //var vuMark = Instantiate(VuMarkPrefab);

                int counter = 0;

                IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetTrackableBehaviours();
                foreach (TrackableBehaviour tb in tbs)
                {
                    if (tb.name == "New Game Object")
                    {

                        // change generic name to include trackable name
                        tb.gameObject.name = ++counter + ":DynamicImageTarget-" + tb.TrackableName;

                        // add additional script components for trackable
                        //tb.gameObject.AddComponent<DefaultTrackableEventHandler>();
                        tb.gameObject.AddComponent<TurnOffBehaviour>();

                        var vb = tb.gameObject.GetComponent<VuMarkBehaviour>();
                        
                        if (vb)
                        {
                            var h = tb.gameObject.AddComponent<VuMarkTrackableEventHandler>();

                            h.TargetName = tb.TrackableName;
                            h.DatabaseId = database.Id;
                        }
                        else
                        {
                            var ib = tb.gameObject.GetComponent<ImageTargetBehaviour>();

                            if (ib)
                            {
                                var h = tb.gameObject.AddComponent<ImageTargetTrackableEventHandler>();

                                h.TargetName = tb.TrackableName;
                                h.DatabaseId = database.Id;
                            }
                        }
                        
                        //tb.gameObject.AddComponent<SphereCollider>();
                    }
                }
            }
            else
            {
                Debug.LogError("<color=yellow>Failed to load dataset: '" + fileName + "'</color>");
            }
        }

        public override void RegisterMarker(IVuforiaMarker marker)
        {
            if (marker != null && marker.Database != null)
            {
                LoadDataSet(marker.Database);
            }
        }

        internal void StartTracking(VuforiaTrackableEventHandler trackable)
        {
            base.OnStartedTracking(trackable.Identifier.ToString(), trackable.gameObject);
        }

        internal void StopTracking(VuforiaTrackableEventHandler trackable)
        {
            base.OnLostTracking(trackable.Identifier.ToString(), trackable.gameObject);
        }
    }
}

#endif