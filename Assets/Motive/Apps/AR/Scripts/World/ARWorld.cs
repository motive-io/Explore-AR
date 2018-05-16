// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive._3D.Models;
using Motive.AR.LocationServices;
using Motive.AR.Models;
using Motive.Core.Models;
using Motive.Core.Scripting;
using Motive.Core.Utilities;
using Motive.UI;
using Motive.Unity.Models;
using Motive.Unity.Scripting;
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using Motive.Unity.World;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Logger = Motive.Core.Diagnostics.Logger;

#if MOTIVE_VUFORIA
using Motive.AR.Vuforia;
#endif

namespace Motive.Unity.AR
{
    /// <summary>
    /// This class manages all AR objects in a scene.
    /// </summary>
    public class ARWorld : SingletonComponent<ARWorld>
    {
        abstract class ARWorldObjectWrapper
        {
            public abstract IARWorldAdapter GetAdapter();
            public abstract ARWorldObject GetWorldObject();
            public abstract void RemoveObject();
            public abstract void AddObject();
            public abstract double GetDistance();
            public abstract Camera GetCamera();

            public List<Action> OnActivateCalls { get; private set; }
            public List<Action> OnDeactivateCalls { get; private set; }

            public virtual Bounds GetBounds()
            {
                var gameObj = GetWorldObject().GameObject;

                Bounds bounds = new Bounds();
                bool hasBounds = false;

                if (gameObj)
                {
                    var renderers = gameObj.GetComponentsInChildren<Renderer>();

                    foreach (var renderer in renderers)
                    {
                        // Ignore particles when computing screen bounds
                        if (renderer is ParticleSystemRenderer)
                        {
                            continue;
                        }

                        if (!hasBounds)
                        {
                            hasBounds = true;
                            bounds = renderer.bounds;
                        }
                        else
                        {
                            bounds.Encapsulate(renderer.bounds);
                        }
                    }
                }

                return bounds;
            }

            public ARWorldObjectWrapper()
            {
                OnActivateCalls = new List<Action>();
                OnDeactivateCalls = new List<Action>();
            }
        }

        class ARWorldObjectWrapper<T> : ARWorldObjectWrapper
            where T : ARWorldObject
        {
            public T WorldObject { get; private set; }
            public IARWorldAdapter<T> Adapter { get; private set; }

            public ARWorldObjectWrapper(T worldObj, IARWorldAdapter<T> adapter)
                : base()
            {
                WorldObject = worldObj;
                Adapter = adapter;
            }

            public override void RemoveObject()
            {
                Adapter.RemoveWorldObject(WorldObject);
            }

            public override void AddObject()
            {
                Adapter.AddWorldObject(WorldObject);
            }

            public override double GetDistance()
            {
                return Adapter.GetDistance(WorldObject);
            }

            public override ARWorldObject GetWorldObject()
            {
                return WorldObject;
            }

            public override IARWorldAdapter GetAdapter()
            {
                return Adapter;
            }

            public override Camera GetCamera()
            {
                return Adapter.GetCameraForObject(WorldObject);
            }
        }

        class ARFence
        {
            public string RequestId { get; set; }
            public ARWorldObject WorldObject { get; set; }
            public DoubleRange Range { get; set; }
            public Action OnInRange { get; set; }
            public Action OnOutOfRange { get; set; }

            public bool? IsInRange { get; set; }
        }

        class WaitAnchorWrapper
        {
            public string AnchorInstanceId;
            public string CallerId;
            public Action<ARWorldObject> OnFound;
        }

        public bool IsActive { get; private set; }
        public bool IsInitialized { get; private set; }

        public UnityEvent OnUpdated;

        //private bool m_doUpdate;
        private SetDictionary<ARWorldObject, ARFence> m_worldObjectFences;
        private Dictionary<ARWorldObject, ARWorldObjectWrapper> m_worldObjDict;
        private SetDictionary<string, ARWorldObjectWrapper> m_resourceWorldObjDictionary;
        private SetDictionary<string, WaitAnchorWrapper> m_waitingAnchorObjects;
        private Dictionary<string, GameObject> m_objectGroups;
        private Dictionary<Type, IARWorldAdapter> m_worldAdapters;

        public AugmentedImageObject AugmentedImageObject;
        public Augmented3dAssetObject Augmented3dObject;

        public AugmentedMediaObject AugmentedVideoObject;
        public AugmentedMediaObject AugmentedAudioObject;

        public LocationARWorldAdapterBase DefaultAdapter;
        public LocationARWorldAdapterBase ARCoreAdapter;
        public LocationARWorldAdapterBase ARKitAdapter;

        public ARWorldAdapterBase<VisualMarkerWorldObject> VisualMarkerAdapter;

        // radius to interact with nodes
        public float DistanceScale = 1f;
        public float DefaultFixedDistance = 5f;

        public bool ActiveOnWake;

        public ARWorldObject SelectedObject { get; private set; }

        private ARWorldObject m_gazeObject;
        private ARWorldObject m_focusObject;

        protected Logger m_logger;

        public Camera MainCamera
        {
            get
            {
                return ARAdapter ? ARAdapter.WorldCamera : null;
            }
        }

        public LocationARWorldAdapterBase ARAdapter;

        public void Activate()
        {
            IsActive = true;

            if (IsInitialized)
            {
                if (ARAdapter)
                {
                    ARAdapter.Activate();
                }

                if (VisualMarkerAdapter != null)
                {
                    VisualMarkerAdapter.Activate();
                }
            }

            foreach (var wrapper in m_worldObjDict.Values.ToArray())
            {
                foreach (var call in wrapper.OnActivateCalls.ToArray())
                {
                    call();
                }
            }
        }

        public void Deactivate()
        {
            IsActive = false;

            if (ARAdapter)
            {
                ARAdapter.Deactivate();
            }

            //m_doUpdate = false;

            if (VisualMarkerAdapter != null)
            {
                VisualMarkerAdapter.Deactivate();
            }

            foreach (var wrapper in m_worldObjDict.Values.ToArray())
            {
                foreach (var call in wrapper.OnDeactivateCalls.ToArray())
                {
                    call();
                }
            }
        }

        void SetARAdapter(LocationARWorldAdapterBase adapter, bool activate)
        {
            ARAdapter.transform.SetParent(this.gameObject.transform);
            ARAdapter.transform.localPosition = Vector3.zero;

            m_worldAdapters[typeof(LocationARWorldObject)] = ARAdapter;
            m_worldAdapters[typeof(SceneARWorldObject)] = ARAdapter;

            if (activate)
            {
                ARAdapter.Activate();
            }
            else
            {
                ARAdapter.Deactivate();
            }
        }


#if MOTIVE_ARCORE
        void CheckARCoreAvailable()
        {
            try
            {
                m_logger.Debug("Checking ARCore availability... {0}", GoogleARCore.Session.Status);

                var result = GoogleARCore.Session.CheckApkAvailability()
                    .ThenAction((status) =>
                    {
                        Debug.LogErrorFormat("Checking ARCore availability result={0}", status);

                        if (status == GoogleARCore.ApkAvailabilityStatus.SupportedInstalled)
                        {
                            ARAdapter = Instantiate(ARCoreAdapter);
                        }
                        else
                        {
                            ARAdapter = Instantiate(DefaultAdapter);
                        }

                        if (ARAdapter != null)
                        {
                            SetARAdapter(ARAdapter, IsActive);
                        }
                    });

                m_logger.Debug("Status info after call: {0} c={1} {2}",
                    GoogleARCore.Session.Status, result.IsComplete, result.Result);
            }
            catch (Exception)
            {
                m_logger.Debug("Couldn't find ARCore DLL, using default");

                if (DefaultAdapter)
                {
                    ARAdapter = Instantiate(DefaultAdapter);

                    SetARAdapter(DefaultAdapter, IsActive);
                }
            }
        }
#endif

        protected override void Awake()
        {
            base.Awake();

            m_worldObjDict = new Dictionary<ARWorldObject, ARWorldObjectWrapper>();
            m_worldAdapters = new Dictionary<Type, IARWorldAdapter>();
            m_resourceWorldObjDictionary = new SetDictionary<string, ARWorldObjectWrapper>();
            m_objectGroups = new Dictionary<string, GameObject>();
            m_worldObjectFences = new SetDictionary<ARWorldObject, ARFence>();
            m_waitingAnchorObjects = new SetDictionary<string, WaitAnchorWrapper>();

            m_logger = new Logger(this);
            
            if (OnUpdated == null)
            {
                OnUpdated = new UnityEvent();
            }

            if (!ARAdapter)
            {
                bool checkSetDefault = true;

                if (UnityEngine.Application.isMobilePlatform)
                {
#if UNITY_ANDROID && MOTIVE_ARCORE
                    if (ARCoreAdapter)
                    {
                        checkSetDefault = false;

                        CheckARCoreAvailable();
                    }
                    else
                    {
                        ARAdapter = Instantiate(DefaultAdapter);
                    }
#endif
#if UNITY_IOS && MOTIVE_ARKIT
                    if (ARKitAdapter && ARKitHelper.IsSupported())
					{
						ARAdapter = Instantiate(ARKitAdapter);
					}
					else
					{
						ARAdapter = Instantiate(DefaultAdapter);
					}
#endif
                }

                if (checkSetDefault && !ARAdapter && DefaultAdapter)
                {
                    ARAdapter = Instantiate(DefaultAdapter);
                }
            }

            IsActive = ActiveOnWake;

            if (ARAdapter != null)
            {
                SetARAdapter(ARAdapter, ActiveOnWake);
            }

            //AddObjectGroup("default");

            //GetComponentInChildren<Renderer>().material.mainTexture = CameraManager.Instance._camera.LivePreview.texture;
        }

        protected override void Start ()
        {
#if MOTIVE_VUFORIA
            VisualMarkerAdapter = VuforiaWorld.Instance;

            if (VisualMarkerAdapter)
            {
                m_worldAdapters[typeof(VisualMarkerWorldObject)] = VisualMarkerAdapter;
            }

            VisualMarkerAdapter.Initialize();
#endif

			ARAdapter.Initialize();

            // Caller may have activated ARWorld before we were initialized
            if (ActiveOnWake || IsActive)
			{
                IsActive = true;

                if (ARAdapter)
                {
                    ARAdapter.Activate();
                }

                if (VisualMarkerAdapter)
                {
                    VisualMarkerAdapter.Activate();
                }
            }
			else
			{
                IsActive = false;

                if (ARAdapter)
                {
                    ARAdapter.Deactivate();
                }

                if (VisualMarkerAdapter)
                {
                    VisualMarkerAdapter.Deactivate();
                }
            }

            IsInitialized = true;

			base.Start ();
		}

        /*
        GameObject AddObjectGroup(string name)
        {
            var objGroup = new GameObject(name);
            objGroup.transform.SetParent(WorldAnchor.transform);

            objGroup.transform.localPosition = Vector3.zero;
            objGroup.transform.localRotation = Quaternion.identity;
            objGroup.transform.localScale = Vector3.one;

            m_objectGroups[name] = objGroup;

            return objGroup;
        }*/

        public LocationAugmentedOptions GetDefaultImageOptions(DoubleRange visibleRange = null)
        {
            return LocationAugmentedOptions.GetLinearDistanceOptions(true, visibleRange);
            //return LocationAugmentedOptions.GetFixedDistanceOptions(DefaultFixedDistance, true, visibleRange);
        }

        public IARWorldAdapter<T> GetAdapter<T>() where T : ARWorldObject
        {
            if (m_worldAdapters.ContainsKey(typeof(T)))
            {
                return ((IARWorldAdapter<T>)m_worldAdapters[typeof(T)]);
            }

            return null;
        }

        void DiscardObjectWrapper(ARWorldObjectWrapper wrapper)
        {
            var worldObject = wrapper.GetWorldObject();

            m_worldObjDict.Remove(worldObject);

            wrapper.RemoveObject();

            if (!worldObject.GameObject) return;

            worldObject.GameObject.transform.SetParent(null);

            Destroy(worldObject.GameObject);
        }

        public void RemoveWorldObject(ARWorldObject worldObject)
        {
            m_logger.Debug("Remove world object");

            ARWorldObjectWrapper wrapper;

            if (m_worldObjDict.TryGetValue(worldObject, out wrapper))
            {
                DiscardObjectWrapper(wrapper);
            }
        }

        public void CreateFence(
            string requestId, 
            ARWorldObject worldObj,
            DoubleRange range,
            Action onInRange,
            Action onOutOfRange)
        {
            var fence = new ARFence
            {
                RequestId = requestId,
                WorldObject = worldObj,
                Range = range,
                OnInRange = onInRange,
                OnOutOfRange = onOutOfRange
            };

            m_worldObjectFences.Add(worldObj, fence);
        }

        public void DiscardFence(ARWorldObject worldObj, string requestId)
        {
            m_worldObjectFences.RemoveWhere(worldObj, f => f.RequestId == requestId);
        }

        public IEnumerable<ARWorldObject> GetWorldObjects(string instanceId)
        {
            var objs = m_resourceWorldObjDictionary[instanceId];

            if (objs != null)
            {
                return objs.Select(w => w.GetWorldObject());
            }

            return null;
        }

        public LocationARWorldObject GetLocationARObject(string instanceId, Location location)
        {
            var objs = GetWorldObjects(instanceId).OfType<LocationARWorldObject>();

            if (objs != null)
            {
                return objs.Where(o => o.Location == location)
                    .FirstOrDefault();
            }

            return null;
        }

        public LocationARWorldObject GetNearestLocationARObject(string instanceId)
        {
            var objs = m_resourceWorldObjDictionary[instanceId].OfType<LocationARWorldObject>();

            if (objs != null)
            {
                if (ForegroundPositionService.Instance.HasLocationData)
                {
                    var coords = ForegroundPositionService.Instance.Position;

                    return objs
                        .OrderBy(o => coords.GetDistanceFrom(o.Location.Coordinates))
                        .FirstOrDefault();
                }
                else
                {
                    return objs.FirstOrDefault();
                }
            }

            return null;
        }

        ARWorldObjectWrapper<T> CreateWorldObjectWrapper<T>(T worldObject, string groupName = "default")
            where T : ARWorldObject
        {
            var adapter = GetAdapter<T>();

            if (adapter == null)
            {
                throw new NotSupportedException("No adapter registered for AR object type " + typeof(T));
            }

            var wrapper = new ARWorldObjectWrapper<T>(worldObject, adapter);

            m_worldObjDict.Add(worldObject, wrapper);

            /****
             * Object groups not currently supported
            GameObject parent = null;

            if (!m_objectGroups.TryGetValue(groupName, out parent))
            {
                parent = AddObjectGroup(groupName);
            }

            worldObject.GameObject.transform.SetParent(parent.transform);

            SetPosition(worldObject);
             */

            return wrapper;
        }

        public void AddWorldObject<T>(T worldObject, string groupName = "default")
            where T : ARWorldObject
        {
            CreateWorldObjectWrapper(worldObject, groupName).AddObject();
        }
        
        private ARWorldObjectWrapper<T> CreateARMedia<T>(T worldObj, MediaElement mediaElement, IARMediaPlaybackProperties properties, Action onClose = null)
            where T : ARWorldObject
        {
            var wrapper = CreateWorldObjectWrapper(worldObj);

            if (mediaElement.MediaItem == null)
            {
                return null;
            }

            AugmentedMediaObject annotationObj = null;

            Action playCall = null;
            Action pauseCall = null;

            Action onReady = null;

            switch (mediaElement.MediaItem.MediaType)
            {
                case Motive.Core.Media.MediaType.Audio:
                    {
                        annotationObj = Instantiate(AugmentedAudioObject);

                        onReady = () =>
                        {
                            var player = annotationObj.GetComponentInChildren<AudioSubpanel>();

                            bool didPlay = false;

                            playCall = () =>
                            {
                                if (!didPlay)
                                {
                                    player.Play(mediaElement.MediaUrl, onClose);

                                    didPlay = true;
                                }
                                else
                                {
                                    player.Play();
                                }
                            };

                            pauseCall = () =>
                            {
                                player.Pause();
                            };
                        };

                        break;
                    }
                case Motive.Core.Media.MediaType.Image:
                    {
                        annotationObj = Instantiate(AugmentedImageObject);

                        onReady = () =>
                        {
                            ThreadHelper.Instance.StartCoroutine(
                                ImageLoader.LoadImage(mediaElement.MediaUrl, annotationObj.RenderObject));
                        };

                        break;
                    }
                case Motive.Core.Media.MediaType.Video:
                    {
                        annotationObj = Instantiate(AugmentedVideoObject);

                        var renderer = annotationObj.RenderObject.GetComponent<Renderer>();

                        renderer.enabled = false;

                        onReady = () =>
                        {
                            var player = annotationObj.GetComponentInChildren<VideoSubpanel>();

                            UnityAction setAspect = () =>
                            {
                                renderer.enabled = true;

                                if (mediaElement.Layout == null)
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

                            bool didPlay = false;

                            if (properties != null)
                            {
                                player.Loop = properties.Loop;
                                player.Volume = properties.Volume;
                            }

                            playCall = () =>
                            {
                                if (!didPlay)
                                {
                                    player.Play(mediaElement.MediaUrl, onClose);

                                    didPlay = true;
                                }
                                else
                                {
                                    player.Play();
                                }
                            };

                            pauseCall = () =>
                            {
                                player.Pause();
                            };
                        };
                        break;
                    }
            }

            if (annotationObj)
            {
                if (mediaElement.Layout != null)
                {
                    LayoutHelper.Apply(annotationObj.LayoutObject.transform, mediaElement.Layout);
                }

                if (mediaElement.Color != null)
                {
                    var renderer = annotationObj.RenderObject.GetComponent<Renderer>();

                    if (renderer)
                    {
                        renderer.material.color = ColorHelper.ToUnityColor(mediaElement.Color);
                    }
                }
            }

            if (annotationObj)
            {
                if (onReady != null)
                {
                    onReady();
                }

                //if (media.OnOpen != null)
                //{
                //    media.OnOpen();
                //}
            }

            if (mediaElement == null)
            {
                return null;
            }

            annotationObj.WorldObject = worldObj;
            worldObj.GameObject = annotationObj.gameObject;

            wrapper.AddObject();

            if (playCall != null && pauseCall != null)
            {
                if (properties != null && properties.OnlyPlayWhenVisible)
                {
                    worldObj.EnteredView += (sender, args) =>
                    {
                        playCall();
                    };

                    worldObj.ExitedView += (sender, args) =>
                    {
                        pauseCall();
                    };

                    wrapper.OnActivateCalls.Add(() =>
                    {
                        if (wrapper.WorldObject.IsVisible)
                        {
                            playCall();
                        }
                    });

                    wrapper.OnDeactivateCalls.Add(pauseCall);
                }
                else
                {
                    wrapper.OnActivateCalls.Add(playCall);
                    wrapper.OnDeactivateCalls.Add(pauseCall);
                }
            }

            return wrapper;
        }

        private ARWorldObjectWrapper<T> CreateARAsset<T>(T worldObj, Motive._3D.Models.AssetInstance assetInstance, Action<ARWorldObjectWrapper<T>, GameObject> onCreate = null)
            where T : ARWorldObject
        {
            var augmentedObject = Instantiate(Augmented3dObject);

            augmentedObject.WorldObject = worldObj;
            worldObj.GameObject = augmentedObject.gameObject;

            var wrapper = CreateWorldObjectWrapper(worldObj);

            if (assetInstance != null)
            {
                AssetLoader.LoadAsset<GameObject>(assetInstance.Asset, (assetObj) =>
                {
                    // augmentedObj could have been destroyed here
                    if (assetObj != null && augmentedObject)
                    {
                        var inst = ObjectHelper.ConfigureAsset(assetObj, assetInstance, augmentedObject.transform);

                        worldObj.TargetObject = assetObj;

                        wrapper.AddObject();

                        if (onCreate != null)
                        {
                            onCreate(wrapper, inst);
                        }
                    }
                });
            }
            else
            {
                wrapper.AddObject();

                onCreate(wrapper, worldObj.GameObject);
            }

            return wrapper;
        }

        private ARWorldObjectWrapper<LocationARWorldObject> CreateLocationARAsset(Location location, double elevation, Motive._3D.Models.AssetInstance assetInstance, ILocationAugmentedOptions options, Action<ARWorldObjectWrapper<LocationARWorldObject>, GameObject> onCreate = null)
        {
            var worldObj = new LocationARWorldObject
            {
                Elevation = elevation,
                Location = location,
                Options = options
            };

            return CreateARAsset(worldObj, assetInstance, onCreate);
        }

        private ARWorldObjectWrapper<SceneARWorldObject> CreateSceneARAsset(ARWorldObject anchorObject, IScriptObject position, Motive._3D.Models.AssetInstance assetInstance, ILocationAugmentedOptions options, Action<ARWorldObjectWrapper<SceneARWorldObject>, GameObject> onCreate = null)
        {
            var worldObj = new SceneARWorldObject
            {
                AnchorObject = anchorObject,
                Position = position,
                Options = options
            };

            return CreateARAsset(worldObj, assetInstance, onCreate);
        }

        private ARWorldObjectWrapper<LocationARWorldObject> CreateLocationARMedia(Location location, MediaElement mediaElement, IARMediaPlaybackProperties properties, ILocationAugmentedOptions options, Action onClose = null)
        {
            var worldObj = new LocationARWorldObject
            {
                Location = location,
                Options = options
            };

            return CreateARMedia(worldObj, mediaElement, properties, onClose);
        }


        private ARWorldObjectWrapper<SceneARWorldObject> CreateSceneMedia(ARWorldObject anchorObject, IScriptObject position, MediaElement mediaElement, IARMediaPlaybackProperties properties, ILocationAugmentedOptions options, Action onClose = null)
        {
            var worldObj = new SceneARWorldObject
            {
                AnchorObject = anchorObject,
                Position = position,
                Options = options
            };

            return CreateARMedia(worldObj, mediaElement, properties, onClose);
        }

        public LocationARWorldObject AddLocationARAsset(Location location, double elevation, Motive._3D.Models.AssetInstance assetInstance, ILocationAugmentedOptions options)
        {
            if (assetInstance == null || assetInstance.Asset == null)
            {
                return null;
            }

            return CreateLocationARAsset(location, elevation, assetInstance, options).WorldObject;
        }

        private ARWorldObjectWrapper<LocationARWorldObject> CreateLocationAugmentedImageObject(Location location, double elevation, string url, ILocationAugmentedOptions options, Layout layout = null, Motive.Core.Models.Color color = null)
        {
            var obj = Instantiate(AugmentedImageObject);

            ThreadHelper.Instance.StartCoroutine(ImageLoader.LoadImage(url, obj.RenderObject));

            var worldObj = new LocationARWorldObject
            {
                Location = location,
                Elevation = elevation,
                GameObject = obj.gameObject,
                Options = options,
            };

            if (layout != null)
            {
                LayoutHelper.Apply(obj.RenderObject.transform, layout);
            }

            obj.WorldObject = worldObj;

            if (color != null)
            {
                var renderer = obj.RenderObject.GetComponent<Renderer>();

                if (renderer)
                {
                    renderer.material.color = ColorHelper.ToUnityColor(color);
                }
            }

            var wrapper = CreateWorldObjectWrapper(worldObj);

            wrapper.AddObject();

            return wrapper;
        }

        public LocationARWorldObject AddLocationARImage(Location location, double elevation, string url, ILocationAugmentedOptions options, Layout layout = null, Motive.Core.Models.Color color = null)
        {
            return CreateLocationAugmentedImageObject(location, elevation, url, options, layout, color).WorldObject;
        }

        void SetUpARWorldObject(ARWorldObjectWrapper objWrapper, ResourceActivationContext context)
        {
            var worldObj = objWrapper.GetWorldObject();

            worldObj.Clicked += (sender, args) =>
            {
                context.FireEvent("select");

                if (ObjectInspectorManager.Instance)
                {
                    ObjectInspectorManager.Instance.Select(context);
                }
            };

            worldObj.GazeEntered += (sender, args) =>
            {
                context.FireEvent("gaze");
                context.SetState("gazing");

                //ObjectInspectorManager.Instance.ObjectAction(context, "gaze");
            };

            worldObj.GazeExited += (sender, args) =>
            {
                context.ClearState("gazing");
                //ObjectInspectorManager.Instance.EndObjectAction(context, "gaze");
            };

            worldObj.Focused += (sender, args) =>
            {
                context.FireEvent("focus");
                context.SetState("in_focus");

                if (ObjectInspectorManager.Instance)
                {
                    ObjectInspectorManager.Instance.ObjectAction(context, "focus");
                }
            };

            worldObj.FocusLost += (sender, args) =>
            {
                context.ClearState("in_focus");

                if (ObjectInspectorManager.Instance)
                {
                    ObjectInspectorManager.Instance.EndObjectAction(context, "focus");
                }
            };

            worldObj.EnteredView += (sender, args) =>
            {
                context.SetState("visible");

                if (ObjectInspectorManager.Instance)
                {
                    ObjectInspectorManager.Instance.SetObjectAvailable(context, true);
                }
            };

            worldObj.ExitedView += (sender, args) =>
            {
                context.ClearState("visible");

                if (ObjectInspectorManager.Instance)
                {
                    ObjectInspectorManager.Instance.SetObjectAvailable(context, false);
                }
            };

            m_resourceWorldObjDictionary.Add(context.InstanceId, objWrapper);

            if (m_waitingAnchorObjects.ContainsKey(context.InstanceId))
            {
                var wrappers = m_waitingAnchorObjects[context.InstanceId].ToArray();

                m_waitingAnchorObjects.RemoveAll(context.InstanceId);

                foreach (var wrapper in wrappers)
                {
                    wrapper.OnFound(worldObj);
                }
            }

            OnUpdated.Invoke();
        }

        #region RESOURCE HOOKS
        public void AttachResourceEvents(ARWorldObject worldObj, ResourceActivationContext context)
        {
            ARWorldObjectWrapper wrapper = null;

            if (m_worldObjDict.TryGetValue(worldObj, out wrapper))
            {
                SetUpARWorldObject(wrapper, context);
            }
        }

        public void AddLocationAugmentedImage(ResourceActivationContext context, LocationAugmentedImage resource, IEnumerable<Location> locations = null)
        {
            locations = locations ?? resource.Locations;

            if (resource.Marker == null || locations == null)
            {
                return;
            }

            foreach (var loc in locations)
            {
                var obj = CreateLocationAugmentedImageObject(loc, resource.Elevation, resource.Marker.MediaUrl, resource, resource.Marker.Layout, resource.Marker.Color);

                if (obj != null)
                {
                    var renderObj = obj.WorldObject.GameObject.GetComponent<AugmentedImageObject>().RenderObject;

                    WorldObjectManager.Instance.AddWorldObject(context, renderObj, renderObj, obj.WorldObject.GameObject);

                    SetUpARWorldObject(obj, context);
                }
            }
        }

        public void RemoveLocationAugmentedImage(ResourceActivationContext ctxt, LocationAugmentedImage resource)
        {
            RemoveResourceObjects(ctxt.InstanceId);
        }

        public void AddLocationAugmented3dAsset(ResourceActivationContext context, LocationAugmented3DAsset resource, IEnumerable<Location> locations = null)
        {
            locations = locations ?? resource.Locations;

            if (resource.AssetInstance == null ||
                resource.AssetInstance.Asset == null ||
                locations == null)
            {
                return;
            }

            foreach (var loc in locations)
            {
                var worldObj = CreateLocationARAsset(loc, resource.Elevation, resource.AssetInstance, resource, (_wo, obj) =>
                {
                    WorldObjectManager.Instance.AddWorldObject(context, obj, _wo.WorldObject.GetAnimationTarget(), _wo.WorldObject.GameObject);
                });
                
                SetUpARWorldObject(worldObj, context);
            }
        }

        public void RemoveLocationAugmented3dAsset(string instanceId)
        {
            RemoveResourceObjects(instanceId);
        }

        void GetAnchor(string callerId, string anchorInstanceId, Action<ARWorldObject> onFound)
        {
            var objs = GetWorldObjects(anchorInstanceId);

            if (objs != null)
            {
                var anchorObj = objs.FirstOrDefault();

                onFound(anchorObj);

                return;
            }
            else
            {
                // Wait for this object
                m_waitingAnchorObjects.Add(anchorInstanceId, new WaitAnchorWrapper
                {
                    AnchorInstanceId = anchorInstanceId,
                    CallerId = callerId,
                    OnFound = onFound
                });
            }
        }

        public void AddAugmented3dAsset(ResourceActivationContext context, Augmented3DAsset resource)
        {
            var relPos = resource.Position as RelativeWorldPosition;

            Action<ARWorldObject> createObj = (anchorObj) =>
            {
                var worldObj = CreateSceneARAsset(anchorObj, resource.Position, resource.AssetInstance, resource, (_wo, obj) =>
                {
                    WorldObjectManager.Instance.AddWorldObject(context, obj, _wo.WorldObject.GetAnimationTarget(), _wo.WorldObject.GameObject);
                });

                SetUpARWorldObject(worldObj, context);
            };
            
            if (relPos != null && relPos.AnchorObjectReference != null)
            {
                var anchorId = context.GetInstanceId(relPos.AnchorObjectReference.ObjectId);

                GetAnchor(context.InstanceId, anchorId, (anchorObj) =>
                {
                    createObj(anchorObj);
                });
            }
            else
            {
                createObj(null);
            }
        }

        public void RemoveAugmented3dAsset(string instanceId)
        {
            RemoveResourceObjects(instanceId);
        }

        public void AddAugmentedMedia(ResourceActivationContext context, AugmentedMedia resource)
        {
            var relPos = resource.Position as RelativeWorldPosition;

            Action<ARWorldObject> createObj = (anchorObj) =>
            {
                var worldObj = CreateSceneMedia(anchorObj, resource.Position, resource.MediaElement, resource, resource, () =>
                {
                    context.Close();
                });

                var obj = worldObj.WorldObject.GameObject;

                var mediaObj = obj.GetComponent<AugmentedMediaObject>();

                WorldObjectManager.Instance.AddWorldObject(context, mediaObj.RenderObject, mediaObj.RenderObject, obj);
                SetUpARWorldObject(worldObj, context);
            };

            if (relPos != null && relPos.AnchorObjectReference != null)
            {
                var anchorId = context.GetInstanceId(relPos.AnchorObjectReference.ObjectId);

                GetAnchor(context.InstanceId, anchorId, (anchorObj) =>
                {
                    createObj(anchorObj);
                });
            }
            else
            {
                createObj(null);
            }
        }

        public void AddLocationAugmentedMedia(ResourceActivationContext context, LocationAugmentedMedia resource)
        {
            if (resource.MediaElement == null ||
                resource.MediaElement.MediaItem == null ||
                resource.Locations == null)
            {
                return;
            }

            foreach (var loc in resource.Locations)
            {
                var worldObj = CreateLocationARMedia(loc, resource.MediaElement, resource, resource, () =>
                {
                    context.Close();
                });

                var obj = worldObj.WorldObject.GameObject;

                var mediaObj = obj.GetComponent<AugmentedMediaObject>();

                WorldObjectManager.Instance.AddWorldObject(context, mediaObj.RenderObject, mediaObj.RenderObject, obj);
                SetUpARWorldObject(worldObj, context);
            }

        }

        public void RemoveAugmentedMedia(string instanceId)
        {
            RemoveResourceObjects(instanceId);
        }

        void RemoveResourceObjects(string instanceId)
        {
            var objs = m_resourceWorldObjDictionary[instanceId];

            if (objs != null)
            {
                foreach (var wo in objs)
                {
                    DiscardObjectWrapper(wo);
                }
            }

            var toRemove = m_waitingAnchorObjects.
                SelectMany(kv => kv.Value.Where(w => w.CallerId == instanceId)).
                ToArray();

            foreach (var wrapper in toRemove)
            {
                m_waitingAnchorObjects.Remove(wrapper.AnchorInstanceId, wrapper);
            }

            WorldObjectManager.Instance.RemoveWorldObjects(instanceId);

            m_resourceWorldObjDictionary.RemoveAll(instanceId);

            OnUpdated.Invoke();
        }
        #endregion

        public void ShowOnly(params string[] groups)
        {
            foreach (var kv in m_objectGroups)
            {
                kv.Value.SetActive(groups.Contains(kv.Key));
            }
        }

        public void ShowAllObjects()
        {
            foreach (var obj in m_objectGroups.Values)
            {
                obj.SetActive(true);
            }
        }

        internal void HideAllObjects()
        {
            foreach (var obj in m_objectGroups.Values)
            {
                obj.SetActive(false);
            }
        }

        bool IsCloser(Vector3 pos, Vector3 comparedTo)
        {
            var d1 = new Vector2(pos.x - .5f, pos.y - .5f).sqrMagnitude;
            var d2 = new Vector2(comparedTo.x - .5f, comparedTo.y - .5f).sqrMagnitude;

            return d1 < d2;
        }

        bool IsVisible(Vector3 pos)
        {
            return pos.z > 0 && (pos.x >= 0 && pos.x <= 1) && (pos.y >= 0 && pos.y <= 1);
        }

        public static Rect GUIRectWithObject(Camera worldCamera, Bounds bounds)
        {
            Vector3 cen = bounds.center;
            Vector3 ext = bounds.extents;

            Vector2[] extentPoints = new Vector2[8]
             {
               WorldToGUIPoint(worldCamera, new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
               WorldToGUIPoint(worldCamera, new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
               WorldToGUIPoint(worldCamera, new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
               WorldToGUIPoint(worldCamera, new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
               WorldToGUIPoint(worldCamera, new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
               WorldToGUIPoint(worldCamera, new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
               WorldToGUIPoint(worldCamera, new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
               WorldToGUIPoint(worldCamera, new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
             };
            Vector2 min = extentPoints[0];
            Vector2 max = extentPoints[0];
            foreach (Vector2 v in extentPoints)
            {
                min = Vector2.Min(min, v);
                max = Vector2.Max(max, v);
            }
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        public static Vector2 WorldToGUIPoint(Camera worldCamera, Vector3 world)
        {
            //Vector2 screenPoint = worldCamera.WorldToScreenPoint(world);
            Vector2 screenPoint = worldCamera.WorldToViewportPoint(world);
            //screenPoint.y = (float)Screen.height - screenPoint.y;
            return screenPoint;
        }

        public Camera GetWorldObjectCamera(ARWorldObject worldObject)
        {
            ARWorldObjectWrapper wrapper = null;

            if (m_worldObjDict.TryGetValue(worldObject, out wrapper))
            {
                return wrapper.GetCamera();
            }

            return this.MainCamera;
        }

        public Bounds GetScreenBounds(ARWorldObject worldObject)
        {
            ARWorldObjectWrapper wrapper = null;

            if (worldObject.GameObject != null &&
                m_worldObjDict.TryGetValue(worldObject, out wrapper))
            {
                var camera = wrapper.GetCamera();
                var bounds = wrapper.GetBounds();

                var pos = camera.WorldToViewportPoint(bounds.center);
                var ext = new Vector3(bounds.extents.x, bounds.extents.y, 0);

                var xbl = bounds.center - ext;
                var xtr = bounds.center + ext;

                var bottomLeft = camera.WorldToViewportPoint(xbl);
                var topRight = camera.WorldToViewportPoint(xtr);

                var forRef = GUIRectWithObject(camera, bounds);

                //return new Bounds(pos, topRight - bottomLeft);
                return new Bounds(forRef.center, forRef.size);
            }

            return new Bounds();
        }

        public Vector3 GetScreenPosition(ARWorldObject worldObject)
        {
            ARWorldObjectWrapper wrapper = null;

            if (worldObject.GameObject != null &&
                m_worldObjDict.TryGetValue(worldObject, out wrapper))
            {
                var pos =
                    wrapper.GetCamera().WorldToViewportPoint(worldObject.GameObject.transform.position);

                return pos;
            }

            return new Vector3(-1, -1, -1);
        }

        // Update is called once per frame
        void Update()
        {
            /*
            if (m_doUpdate)
            {
                foreach (var obj in m_worldObjects)
                {
                    SetPosition(obj);
                }
            }*/

            if (!MainCamera || !IsActive)
            {
                return;
            }

            // Find out what's in focus

            var w = Screen.width / 2;
            var h = Screen.height / 2;

            var ray = MainCamera.ScreenPointToRay(new Vector2(w, h));

            RaycastHit hitInfo;

            GameObject rayHit = null;

            if (Physics.Raycast(ray, out hitInfo))
            {
                rayHit = hitInfo.collider.gameObject;
            }

            ARWorldObject nearest = null;
            ARWorldObject rayHitObj = null;

            Vector3 nearestPos = Vector3.zero;

            List<Action> calls = null;

            Action<Action> addCall = (c) =>
            {
                if (calls == null)
                {
                    calls = new List<Action>();
                }

                calls.Add(c);
            };
            
            // Find out if anything is on screen, choose the closest to middle
            foreach (var wrapper in m_worldObjDict.Values)
            {
                var camera = wrapper.GetCamera();
                var worldObj = wrapper.GetWorldObject();
                var gameObj = worldObj.GameObject;
                
                if (gameObj && gameObj.activeSelf && camera && camera.isActiveAndEnabled)
                {
                    var pos = camera.WorldToViewportPoint(gameObj.transform.position);

                    var vis = IsVisible(pos);

                    if (worldObj.IsVisible != vis)
                    {
                        addCall(() => worldObj.SetVisible(vis));
                    }

                    if (gameObj == rayHit)
                    {
                        rayHitObj = worldObj;
                    }

                    // Focus object is the gaze object or next-nearest object.
                    // If we already have a focus object, don't bother trying to
                    // compute the next nearest one.
                    if (vis && rayHitObj == null)
                    {
                        if (nearest == null || IsCloser(pos, nearestPos))
                        {
                            nearest = worldObj;
                            nearestPos = pos;
                        }
                    }

                    var fences = m_worldObjectFences[worldObj];

                    if (fences != null)
                    {
                        var d = wrapper.GetDistance();

                        foreach (var fence in fences)
                        {
                            if (fence.Range != null)
                            {
                                if (fence.Range.IsInRange(d) && gameObj.activeSelf)
                                {
                                    // if "IsInRange" isn't set OR
                                    // fence is not currently in range
                                    // then signal "on in range"
                                    if (!fence.IsInRange.HasValue ||
                                        !fence.IsInRange.Value)
                                    {
                                        fence.IsInRange = true;
                                        
                                        if (fence.OnInRange != null)
                                        {
                                            addCall(() => fence.OnInRange());                                            
                                        }
                                    }
                                }
                                else
                                {
                                    // if "IsInRange" isn't set OR
                                    // fence is currently in range
                                    // then signal "on out of range"
                                    if (!fence.IsInRange.HasValue ||
                                        fence.IsInRange.Value)
                                    {
                                        fence.IsInRange = false;

                                        if (fence.OnOutOfRange != null)
                                        {
                                            addCall(() => fence.OnOutOfRange());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    addCall(() => worldObj.SetVisible(false));
                }
            }

            if (calls != null)
            {
                calls.ForEach(c => c());
            }

            if (rayHitObj != null)
            {
                nearest = rayHitObj;
            }

            if (m_gazeObject != null)
            {
                if (rayHitObj != m_gazeObject)
                {
                    m_gazeObject.OnGazeExit();
                    m_gazeObject = null;

                    if (rayHitObj != null)
                    {
                        m_gazeObject = rayHitObj;
                        m_gazeObject.OnGazeEnter();
                    }
                }
            }
            else if (rayHitObj != null)
            {
                m_gazeObject = rayHitObj;
                m_gazeObject.OnGazeEnter();
            }

            if (m_focusObject != null)
            {
                if (nearest != m_focusObject)
                {
                    m_focusObject.OnFocusLost();
                    m_focusObject = null;

                    if (nearest != null)
                    {
                        m_focusObject = nearest;
                        m_focusObject.OnFocus();
                    }
                }
            }
            else if (nearest != null)
            {
                m_focusObject = nearest;
                m_focusObject.OnFocus();
            }

            WorldObjectProximityConditionMonitor.Instance.Update();
        }

        public void Select(ARWorldObject obj)
        {
            var lastSelected = SelectedObject;
            SelectedObject = obj;

            if (lastSelected != obj)
            {
                if (obj != null)
                {
                    obj.OnSelect();
                }

                if (lastSelected != null)
                {
                    lastSelected.OnDeselect();
                }
            }
        }

        internal float GetDistance(ARWorldObject worldObj)
        {
            ARWorldObjectWrapper wrapper;

            if (m_worldObjDict.TryGetValue(worldObj, out wrapper))
            {
                return (float)wrapper.GetDistance();
            }

            return -1;
        }
    }
}
