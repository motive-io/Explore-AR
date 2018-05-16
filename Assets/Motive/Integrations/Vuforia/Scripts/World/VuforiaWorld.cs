// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.AR.Models;
using Motive.Core.Scripting;
using UnityEngine;
using Motive.Unity.Utilities;
using Motive.Unity.UI;
using Motive.Core.Models;
using Motive.AR;
using Motive._3D.Models;
using Motive.UI;

using Logger = Motive.Core.Diagnostics.Logger;
using Motive.Core.Utilities;
using Motive.Unity;
using UnityEngine.Events;
using Motive.Core.Media;
using System.Collections;
using Motive;
using Motive.Unity.AR;
using Motive.Unity.World;
using Motive.Unity.Models;

#if MOTIVE_VUFORIA
using Vuforia;

namespace Motive.AR.Vuforia
{
    public class VuforiaWorld : ARWorldAdapterBase<VisualMarkerWorldObject>
    {
        public enum CameraRunningBehavior
        {
            OnlyWhileActive,
            AlwaysOn,
            AlwaysOnDelayedInit
        };

        public VuforiaMarkerTrackingConditionMonitor TrackingConditionMonitor { get; private set; }

        public VuforiaBehaviour ARCamera;
        public GameObject InitializingPane;

        public UnityEvent Activated;
        public UnityEvent Deactivated;

        public bool IsInitialized { get; private set; }

        public bool ActivateOnWake;
        //public bool AlwaysOn;

        public CameraRunningBehavior CameraBehavior;
         
        bool m_doDeactivate;

        public VisualMarkerObject MarkerImageObject;
        public VisualMarkerObject MarkerVideoObject;
        public VisualMarkerObject MarkerAudioObject;
        public VisualMarkerObject MarkerAssetObject;

        static VuforiaWorld g_instance;

        public static VuforiaWorld Instance
        {
            get
            {
                return g_instance;
            }
        }

        class MarkerResource
        {
            public ResourceActivationContext ActivationContext;
            public string ObjectId;
            public IVuforiaMarker Marker;
            public Action OnOpen;
            public Action OnClose;
            public Action OnSelect;
        }

        class MarkerMedia : MarkerResource
        {
            public string MediaUrl;
            public Motive.Core.Models.Color Color;
            public MediaType MediaType;
            public Layout MediaLayout;
        }

        class MarkerAsset : MarkerMedia
        {
            public AssetInstance AssetInstance;
        }

        class ActiveObject
        {
            public Dictionary<VuMarkIdentifier, VisualMarkerWorldObject> MarkerObjects;

            public HashSet<VuMarkIdentifier> PendingLoads;

            public ActiveObject()
            {
                PendingLoads = new HashSet<VuMarkIdentifier>();
                MarkerObjects = new Dictionary<VuMarkIdentifier, VisualMarkerWorldObject>();
            }
        }

        SetDictionary<VuMarkIdentifier, VuforiaTrackableEventHandler> m_trackingVuMarkers;
        List<VuforiaTrackableEventHandler> m_trackingHold;

        SetDictionary<VuMarkIdentifier, MarkerMedia> m_activeResourceMedia;
        SetDictionary<VuMarkIdentifier, MarkerAsset> m_activeResourceAssets;

        //HashSet<AudioSubpanel> m_playingAudios;
        //HashSet<VideoSubpanel> m_playingVideos;

        Dictionary<string, ActiveObject> m_activeResourceObjects;

        HashSet<string> m_loadedDatabases;
        HashSet<MarkerDatabase> m_toLoad;

        Action m_onStart;

        protected override void Awake()
        {
            base.Awake();

            g_instance = this;

            m_trackingVuMarkers = new SetDictionary<VuMarkIdentifier, VuforiaTrackableEventHandler>();
            m_trackingHold = new List<VuforiaTrackableEventHandler>();

            m_activeResourceMedia = new SetDictionary<VuMarkIdentifier, MarkerMedia>();
            m_activeResourceAssets = new SetDictionary<VuMarkIdentifier, MarkerAsset>();
            m_activeResourceObjects = new Dictionary<string, ActiveObject>();
            m_loadedDatabases = new HashSet<string>();
            m_toLoad = new HashSet<MarkerDatabase>();

            TrackingConditionMonitor = new VuforiaMarkerTrackingConditionMonitor();

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
            if (ActivateOnWake)
            {
                Activate();
            }
            else if (CameraBehavior == CameraRunningBehavior.AlwaysOn)
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

            base.Initialize();
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

            yield return new WaitForEndOfFrame();

            onReady();
        }

        public override void Activate()
        {
            m_logger.Debug("Activate IsInitialized={0}", IsInitialized);

            base.Activate();

            if (InitializingPane)
            {
                InitializingPane.SetActive(true);
            }
            
            Action onReady = () =>
                {
                    m_logger.Debug("Vuforia initialized");

                    LoadDataSets();

                    foreach (var handler in m_trackingHold)
                    {
                        handler.ResetIdentifier();
                        StartTracking(handler);
                    }

                    m_trackingHold.Clear();

                    if (InitializingPane)
                    {
                        InitializingPane.SetActive(false);
                    }

                    TrackingConditionMonitor.TrackingMarkersUpdated();

                    if (Activated != null)
                    {
                        Activated.Invoke();
                    }
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

        IEnumerable<GameObject> GetActiveObjects()
        {
            foreach (var vm in m_trackingVuMarkers.Values)
            {
                var activeMedias = m_activeResourceMedia[vm.Identifier];

                if (activeMedias != null)
                {
                    foreach (var media in activeMedias)
                    {
                        var objs = m_activeResourceObjects[media.ObjectId];

                        if (objs != null)
                        {
                            foreach (var obj in objs.MarkerObjects.Values)
                            {
                                yield return obj.GameObject;
                            }
                        }
                    }
                }
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();

            m_logger.Debug("Deactivate: IsInitialized = {0}", IsInitialized);

            if (IsInitialized)
            {
                m_doDeactivate = true;

                var handlers = m_trackingVuMarkers.Values.ToArray();

                foreach (var handler in handlers)
                {
                    StopTracking(handler);
                    m_trackingHold.Add(handler);
                }
            }

            if (Deactivated != null)
            {
                Deactivated.Invoke();
            }

            TrackingConditionMonitor.TrackingMarkersUpdated();
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

        public bool IsTracking(IVuforiaMarker marker)
        {
            if (IsActive)
            {
                if (marker.Database != null)
                {
                    LoadDataSet(marker.Database);
                }

                return m_trackingVuMarkers.GetCount(marker.Identifier) > 0;
            }

            return false;
        }

        bool DoesObjectExist(VuMarkIdentifier markerId, string objectId)
        {
            ActiveObject activeObject = null;

            if (m_activeResourceObjects.TryGetValue(objectId, out activeObject))
            {
                return activeObject.MarkerObjects.ContainsKey(markerId);
            }

            return false;
        }

        ActiveObject GetOrCreateActiveObject(string objectId)
        {
            ActiveObject activeObject = null;

            if (!m_activeResourceObjects.TryGetValue(objectId, out activeObject))
            {
                activeObject = new ActiveObject();

                m_activeResourceObjects.Add(objectId, activeObject);
            }

            return activeObject;
        }

        VisualMarkerWorldObject CreateWorldObject(VisualMarkerObject markerObj, GameObject animationTarget, ResourceActivationContext context = null)
        {
            var worldObj = new VisualMarkerWorldObject
            {
                GameObject = markerObj.gameObject
            };

            markerObj.WorldObject = worldObj;

            ARWorld.Instance.AddWorldObject(worldObj);
            WorldObjectManager.Instance.AddWorldObject(context, markerObj.gameObject, animationTarget, markerObj.LayoutObject);

            if (context != null)
            {
                ARWorld.Instance.AttachResourceEvents(worldObj, context);
            }

            return worldObj;
        }

        void AttachAssetObject(MarkerAsset asset, GameObject parent)
        {
            var activeObject = GetOrCreateActiveObject(asset.ObjectId);

            VisualMarkerWorldObject worldObj = null;

            activeObject.MarkerObjects.TryGetValue(asset.Marker.Identifier, out worldObj);

            if (worldObj != null)
            {
                worldObj.GameObject.transform.SetParent(parent.transform, false);
                worldObj.GameObject.transform.localPosition = Vector3.zero;
                worldObj.GameObject.transform.localScale = Vector3.one;
                worldObj.GameObject.transform.localRotation = Quaternion.identity;

                if (asset.OnOpen != null)
                {
                    asset.OnOpen();
                }

                worldObj.GameObject.SetActive(true);
            }
            else
            {
                if (activeObject.PendingLoads.Contains(asset.Marker.Identifier))
                {
                    return;
                }

                activeObject.PendingLoads.Add(asset.Marker.Identifier);

                UnityAssetLoader.LoadAsset<GameObject>(asset.AssetInstance.Asset as UnityAsset, (prefab) =>
                {
                    activeObject.PendingLoads.Remove(asset.Marker.Identifier);

                    // parent object could have been destroyed
                    if (prefab && parent)
                    {
                        var gameObj = Instantiate(prefab);

                        var markerObj = Instantiate(MarkerAssetObject);

                        worldObj = CreateWorldObject(markerObj, gameObj, asset.ActivationContext);

                        worldObj.Clicked += (sender, args) =>
                        {
                            if (asset.OnSelect != null)
                            {
                                asset.OnSelect();
                            }
                        };

                        gameObj.transform.SetParent(markerObj.LayoutObject.transform, false);
                        gameObj.transform.localPosition = Vector3.zero;

                        markerObj.transform.SetParent(parent.transform, false);
                        markerObj.transform.localPosition = Vector3.zero;
                        markerObj.transform.localScale = Vector3.one;
                        markerObj.transform.localRotation = Quaternion.identity;

                        if (asset.AssetInstance.Layout != null)
                        {
                            LayoutHelper.Apply(markerObj.LayoutObject.transform, asset.AssetInstance.Layout);
                        }

                        var collider = gameObj.GetComponent<Collider>();

                        if (!collider)
                        {
                            gameObj.AddComponent<SphereCollider>();
                        }

                        activeObject.MarkerObjects.Add(asset.Marker.Identifier, worldObj);

                        if (asset.OnOpen != null)
                        {
                            asset.OnOpen();
                        }
                    }
                    else
                    {
                        if (asset.MediaUrl != null)
                        {
                            AttachMediaObject(asset, parent);
                        }
                    }
                });
            }
        }

        /*
        void AttachAssetObject(VuMarkIdentifier identifier, GameObject parent, string objectId, AssetInstance assetInstance, MediaElement fallbackImage, Action onOpen, Action onSelect)
        {
            if (fallbackImage != null)
            {
                AttachAssetObject(identifier, parent, objectId, assetInstance, fallbackImage.MediaUrl, fallbackImage.Layout, onOpen, onSelect);
            }
            else
            {
                AttachAssetObject(identifier, parent, objectId, assetInstance, null, null, onOpen);
            }
        }*/

        void AttachMediaObject(
            MarkerMedia media,
            //VuMarkIdentifier identifier,
            GameObject parent
            /*, 
            string objectId, 
            string url, 
            MediaType mediaType, 
            Layout layout, 
            Action onOpen, 
            Action onSelect, 
            Action onComplete*/)
        {
            var activeObject = GetOrCreateActiveObject(media.ObjectId);

            VisualMarkerObject markerObj = null;

            Action onReady = null;

            VisualMarkerWorldObject worldObj = null;

            activeObject.MarkerObjects.TryGetValue(media.Marker.Identifier, out worldObj);

            if (worldObj != null)
            {
                markerObj = worldObj.GameObject.GetComponent<VisualMarkerObject>();

                switch (media.MediaType)
                {
                    case Motive.Core.Media.MediaType.Audio:
                        onReady = () =>
                            {
                                var player = markerObj.GetComponentInChildren<AudioSubpanel>();

                                player.Play(media.OnClose);
                            };

                        break;
                    case Motive.Core.Media.MediaType.Video:
                        onReady = () =>
                            {
                                var player = markerObj.GetComponentInChildren<VideoSubpanel>();

                                player.Play(media.OnClose);
                            };

                        break;
                }

                worldObj.GameObject.SetActive(true);
            }
            else
            {
                switch (media.MediaType)
                {
                    case Motive.Core.Media.MediaType.Audio:
                        {
                            markerObj = Instantiate(MarkerAudioObject);

                            onReady = () =>
                                {
                                    var player = markerObj.GetComponentInChildren<AudioSubpanel>();

                                    player.Play(media.MediaUrl, media.OnClose);
                                };

                            break;
                        }
                    case Motive.Core.Media.MediaType.Image:
                        {
                            markerObj = Instantiate(MarkerImageObject);

                            onReady = () =>
                            {
                                ThreadHelper.Instance.StartCoroutine(
                                    ImageLoader.LoadImage(media.MediaUrl, markerObj.RenderObject));
                            };

                            break;
                        }
                    case Motive.Core.Media.MediaType.Video:
                        {
                            markerObj = Instantiate(MarkerVideoObject);

                            var renderer = markerObj.RenderObject.GetComponent<Renderer>();

                            renderer.enabled = false;

                            onReady = () =>
                                {                       
                                    var player = markerObj.GetComponentInChildren<VideoSubpanel>();

                                    UnityAction setAspect = () =>
                                        {
                                            renderer.enabled = true;

                                            if (media.MediaLayout == null)
                                            {
                                                var aspect = player.AspectRatio;

                                                if (aspect > 1)
                                                {
                                                    // Wider than tall, reduce y scale
                                                    player.transform.localScale =
                                                        new Vector3(1, 1 / aspect, 1);
                                                }
                                                else
                                                {
                                                    // Wider than tall, reduce x scale
                                                    player.transform.localScale = new Vector3(aspect, 1, 1);
                                                }
                                            }

                                        };

                                    player.ClipLoaded.AddListener(setAspect);

                                    player.Play(media.MediaUrl, media.OnClose);
                                };
                            break;
                        }
                }

                if (markerObj)
                {
                    if (media.MediaLayout != null)
                    {
                        LayoutHelper.Apply(markerObj.LayoutObject.transform, media.MediaLayout);
                    }

                    if (media.Color != null)
                    {
                        var renderer = markerObj.RenderObject.GetComponent<Renderer>();

                        if (renderer)
                        {
                            renderer.material.color = ColorHelper.ToUnityColor(media.Color);
                        }
                    }

                    worldObj = CreateWorldObject(markerObj, markerObj.RenderObject, media.ActivationContext);

                    activeObject.MarkerObjects[media.Marker.Identifier] = worldObj;

                    worldObj.Clicked += (sender, args) =>
                    {
                        if (media.OnSelect != null)
                        {
                            media.OnSelect();
                        }
                    };
                }
            }

            if (markerObj)
            {
                markerObj.transform.SetParent(parent.transform, false);
                markerObj.transform.localScale = Vector3.one;
                markerObj.transform.localPosition = Vector3.zero;
                markerObj.transform.localRotation = Quaternion.identity;

                if (onReady != null)
                {
                    onReady();
                }

                if (media.OnOpen != null)
                {
                    media.OnOpen();
                }
            }
        }

        void AttachMediaObject(VuMarkIdentifier identifier, GameObject parent, string objectId, MediaElement mediaElement, Action onOpen, Action onSelect, Action onComplete)
        {
            /*
            AttachMediaObject(
                identifier,
                parent,
                objectId,
                mediaElement.MediaUrl,
                mediaElement.MediaItem.MediaType,
                mediaElement.Layout, 
                onOpen,
                onSelect,
                onComplete);*/
        }

        public void RemoveResourceObjects(string resourceId)
        {
            ActiveObject activeObj = null;

            if (m_activeResourceObjects.TryGetValue(resourceId, out activeObj))
            {
                foreach (var obj in activeObj.MarkerObjects.Values)
                {
                    ARWorld.Instance.RemoveWorldObject(obj);
                }

                m_activeResourceObjects.Remove(resourceId);
            }

            var removeMedias = m_activeResourceMedia.Values.Where(v => v.ObjectId == resourceId).ToArray();

            foreach (var rm in removeMedias)
            {
                m_activeResourceMedia.Remove(rm.Marker.Identifier, rm);
            }

            var removeAssets = m_activeResourceAssets.Values.Where(v => v.ObjectId == resourceId).ToArray();

            foreach (var ra in removeAssets)
            {
                m_activeResourceAssets.Remove(ra.Marker.Identifier, ra);
            }

            WorldObjectManager.Instance.RemoveWorldObjects(resourceId);
        }

        void AddMarkerMedia(MarkerMedia markerMedia)
        {
            if (markerMedia.MediaUrl == null)
            {
                return;
            }

            m_activeResourceMedia.Add(markerMedia.Marker.Identifier, markerMedia);

            var objs = m_trackingVuMarkers[markerMedia.Marker.Identifier];

            if (markerMedia.Marker.Database != null)
            {
                LoadDataSet(markerMedia.Marker.Database);
            }

            if (objs != null)
            {
                foreach (var obj in objs)
                {
                    obj.ExtendedTracking = markerMedia.Marker.UseExtendedTracking;

                    AttachMediaObject(
                        markerMedia,
                        //markerMedia.Marker.Identifier, 
                        obj.gameObject
                        /*, 
                        markerMedia.ObjectId, 
                        markerMedia.MediaUrl,
                        markerMedia.MediaType,
                        markerMedia.MediaLayout,
                        markerMedia.OnOpen,
                        markerMedia.OnSelect,
                        markerMedia.OnClose*/);
                }
            }
        }

        public void Add3dAsset(ResourceActivationContext ctxt, IVuforiaMarker marker, string mediaId, AssetInstance assetInstance, string fallbackImageUrl = null, Layout layout = null, Action onOpen = null, Action onSelect = null)
        {
            if (DoesObjectExist(marker.Identifier, mediaId))
            {
                return;
            }

            var markerAsset = new MarkerAsset
                {
                    ActivationContext = ctxt,
                    AssetInstance = assetInstance,
                    Marker = marker,
                    MediaType = MediaType.Image,
                    MediaUrl = fallbackImageUrl,
                    MediaLayout = layout,
                    ObjectId = mediaId,
                    OnSelect = onSelect,
                    OnOpen = onOpen
                };

            Add3dAsset(markerAsset);
        }

        public void AddMarkerImage(ResourceActivationContext ctxt, IVuforiaMarker marker, string mediaId, string url, Layout layout = null, Action onOpen = null, Action onSelect = null)
        {
            if (DoesObjectExist(marker.Identifier, mediaId))
            {
                return;
            }

            var markerMedia = new MarkerMedia
                {
                    ActivationContext = ctxt,
                    Marker = marker,
                    MediaType = MediaType.Image,
                    MediaUrl = url,
                    MediaLayout = layout,
                    ObjectId = mediaId,
                    OnSelect = onSelect,
                    OnOpen = onOpen
                };

            AddMarkerMedia(markerMedia);
        }

        public void AddMarkerMedia(IVuforiaMarker marker, string mediaId, MediaElement mediaElement, Action onOpen = null, Action onSelect = null, Action onClose = null)
        {
            if (mediaElement == null || mediaElement.MediaUrl == null)
            {
                return;
            }

            var markerMedia = new MarkerMedia
                {
                    Marker = marker,
                    MediaType = mediaElement.MediaItem.MediaType,
                    MediaUrl = mediaElement.MediaUrl,
                    MediaLayout = mediaElement.Layout,
                    Color = mediaElement.Color,
                    ObjectId = mediaId,
                    OnOpen = onOpen,
                    OnSelect = onSelect,
                    OnClose = onClose
                };

            AddMarkerMedia(markerMedia);
        }

        public void AddMarkerMedia(ResourceActivationContext context, VisualMarkerMedia resource)
        {
            if (resource.MediaElement == null ||
                resource.MediaElement.MediaUrl == null ||
                resource.Markers == null)
            {
                return;
            }

            foreach (var marker in resource.Markers)
            {
                var vumark = marker as IVuforiaMarker;

                if (vumark != null)
                {
                    var media = new MarkerMedia
                        {   
                            ActivationContext = context,
                            Marker = vumark,
                            MediaLayout = resource.MediaElement.Layout,
                            MediaType = resource.MediaElement.MediaItem.MediaType,
                            MediaUrl = resource.MediaElement.MediaUrl,
                            Color = resource.MediaElement.Color,
                            ObjectId = context.InstanceId,
                            OnOpen = () =>
                                {
                                    context.Open();
                                },
                            OnSelect = () =>
                                {
                                    context.FireEvent("select");
                                },
                            OnClose = () =>
                                {
                                    context.Close();
                                }
                        };

                    AddMarkerMedia(media);
                }
            }
        }

        internal void RemoveMarkerMedia(ResourceActivationContext context, VisualMarkerMedia resource)
        {
            if (resource.MediaElement == null ||
                resource.MediaElement.MediaUrl == null ||
                resource.Markers == null)
            {
                return;
            }

            foreach (var marker in resource.Markers)
            {
                var vumark = marker as IVuforiaMarker;

                if (vumark != null)
                {
                    m_activeResourceMedia.RemoveWhere(vumark.Identifier, c => c.ObjectId == context.InstanceId);
                }
            }

            RemoveResourceObjects(context.InstanceId);
        }

        internal void Remove3DAsset(ResourceActivationContext context, VisualMarker3DAsset resource)
        {
            if (resource.AssetInstance == null ||
                resource.AssetInstance.Asset == null ||
                resource.Markers == null)
            {
                return;
            }

            foreach (var marker in resource.Markers)
            {
                var vumark = marker as IVuforiaMarker;

                if (vumark != null)
                {
                    m_activeResourceAssets.RemoveWhere(vumark.Identifier, c => c.ObjectId == context.InstanceId);
                }
            }

            RemoveResourceObjects(context.InstanceId);
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

        void Add3dAsset(MarkerAsset markerAsset)
        {
            if (markerAsset.Marker.Database != null)
            {
                LoadDataSet(markerAsset.Marker.Database);
            }

            var objs = m_trackingVuMarkers[markerAsset.Marker.Identifier];

            m_activeResourceAssets.Add(markerAsset.Marker.Identifier, markerAsset);

            if (objs != null && objs.Count() > 0)
            {
                foreach (var obj in objs)
                {
                    obj.ExtendedTracking = markerAsset.Marker.UseExtendedTracking;

                    AttachAssetObject(
                        markerAsset,
                        //markerAsset.Marker.Identifier, 
                        obj.gameObject
                        /*, 
                        markerAsset.ObjectId, 
                        markerAsset.AssetInstance,
                        markerAsset.MediaUrl,
                        markerAsset.MediaLayout,
                        markerAsset.OnOpen,
                        markerAsset.OnSelect*/);
                }
            }
            else
            {
                // Preload the asset even if there's no marker yet
                UnityAssetLoader.LoadAsset<GameObject>((UnityAsset)markerAsset.AssetInstance.Asset, (obj) => { });
            }
        }

        public void Add3dAsset(IVuforiaMarker marker, string objectId, AssetInstance assetInstance, MediaElement fallbackImage)
        {
            var markerAsset = new MarkerAsset
                {
                    AssetInstance = assetInstance,
                    Marker = marker,
                    ObjectId = objectId
                };

            if (fallbackImage != null)
            {
                markerAsset.MediaUrl = fallbackImage.MediaUrl;
                markerAsset.MediaLayout = fallbackImage.Layout;
            }

            Add3dAsset(markerAsset);
        }

        internal void Add3dAsset(ResourceActivationContext context, VisualMarker3DAsset resource)
        {
            if (resource.AssetInstance == null ||
                resource.Markers == null)
            {
                return;
            }

            foreach (var marker in resource.Markers)
            {
                var vumark = marker as IVuforiaMarker;

                if (vumark != null)
                {
                    var asset = new MarkerAsset
                        {
                            ActivationContext = context,
                            AssetInstance = resource.AssetInstance,
                            Marker = vumark,
                            MediaLayout = resource.FallbackImage == null ? null : resource.FallbackImage.Layout,
                            MediaUrl = resource.FallbackImage == null ? null : resource.FallbackImage.MediaUrl,
                            ObjectId = context.InstanceId,
                            OnOpen = () =>
                                {
                                    context.Open();
                                },
                            OnSelect = () =>
                                {
                                    context.FireEvent("select");
                                },
                            OnClose = () =>
                                {
                                    context.Close();
                                }
                        };

                    Add3dAsset(asset);
                }
            }
        }

        internal void StopTracking(VuforiaTrackableEventHandler m_trackableBehaviour)
        {
            if (m_trackingHold.Contains(m_trackableBehaviour))
            {
                m_trackingHold.Remove(m_trackableBehaviour);

                return;
            }

            // Pause all audio/video players
            var audioPlayers = m_trackableBehaviour.GetComponentsInChildren<AudioSubpanel>();
            var videoPlayers = m_trackableBehaviour.GetComponentsInChildren<VideoSubpanel>();

            foreach (var ap in audioPlayers)
            {
                ap.Pause();
            }

            foreach (var vp in videoPlayers)
            {
                vp.Pause();
            }

            List<GameObject> objs = new List<GameObject>();

            for (int i = 0; i < m_trackableBehaviour.gameObject.transform.childCount; i++)
            {
                var t = m_trackableBehaviour.gameObject.transform.GetChild(i);
                objs.Add(t.gameObject);
            }

            foreach (var obj in objs)
            {
                obj.transform.SetParent(null);
                obj.SetActive(false);
            }

            //m_trackableBehaviour.Clicked.RemoveAllListeners();

            if (m_trackableBehaviour.Identifier != null)
            {
                var instanceId = m_trackableBehaviour.Identifier;

                m_logger.Debug("stop tracking " + instanceId);

                m_trackingVuMarkers.Remove(m_trackableBehaviour.Identifier, m_trackableBehaviour);
            }
        }

        internal void StartTracking(VuforiaTrackableEventHandler trackableBehaviour)
        {
            // hack for now: clear any objects attached to this one
            for (int i = 0; i < trackableBehaviour.gameObject.transform.childCount; i++)
            {
                var t = trackableBehaviour.gameObject.transform.GetChild(i);
                //Destroy(t.gameObject);
            }

            var instanceId = trackableBehaviour.Identifier;

            m_logger.Debug("start tracking " + instanceId);

            m_trackingVuMarkers.Add(trackableBehaviour.Identifier, trackableBehaviour);

            /*
        m_trackableBehaviour.Clicked.AddListener(() =>
        {
            m_trackableBehaviour_Clicked(m_trackableBehaviour);
        });*/

            var media = m_activeResourceMedia[trackableBehaviour.Identifier];

            m_logger.Debug("found {0} media items for {1}", media == null ? 0 : media.Count(), instanceId);

            if (media != null)
            {
                foreach (var c in media)
                {
                    trackableBehaviour.ExtendedTracking = c.Marker.UseExtendedTracking;

                    AttachMediaObject(c, trackableBehaviour.gameObject);
                    /*
                        m_trackableBehaviour.Identifier,
                        m_trackableBehaviour.gameObject,
                        c.ObjectId,
                        c.MediaUrl,
                        c.MediaType,
                        c.MediaLayout,
                        c.OnOpen,
                        c.OnSelect,
                        c.OnClose);*/
                }
            }

            var assets = m_activeResourceAssets[trackableBehaviour.Identifier];

            if (assets != null)
            {
                foreach (var a in assets)
                {
                    trackableBehaviour.ExtendedTracking = a.Marker.UseExtendedTracking;

                    AttachAssetObject(
                        a,
                        trackableBehaviour.gameObject);
                    /*
                        m_trackableBehaviour.Identifier,
                        m_trackableBehaviour.gameObject, 
                        a.ObjectId, 
                        a.AssetInstance,
                        a.MediaUrl,
                        a.MediaLayout,
                        a.OnOpen,
                        a.OnSelect);*/
                }
            }
        }

        void m_trackableBehaviour_Clicked(VuforiaTrackableEventHandler trackable)
        {
            var medias = m_activeResourceMedia[trackable.Identifier];

            if (medias != null)
            {
                foreach (var m in medias.ToArray())
                {
                    if (m.OnSelect != null)
                    {
                        m.OnSelect();
                    }

                    ObjectInspectorManager.Instance.Select(m.ObjectId);
                }
            }

            var assets = m_activeResourceAssets[trackable.Identifier];

            if (assets != null)
            {
                foreach (var m in assets.ToArray())
                {
                    if (m.OnSelect != null)
                    {
                        m.OnSelect();
                    }

                    ObjectInspectorManager.Instance.Select(m.ObjectId);
                }
            }
        }

        public override void AttachWorldObject(VisualMarkerWorldObject worldObject, Transform parent)
        {
            //throw new NotImplementedException();
        }
    }
}

#endif