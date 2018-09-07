// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive._3D.Models;
using Motive.AR.Models;
using Motive.AR.Vuforia;
using Motive.Core.Media;
using Motive.Core.Scripting;
using Motive.Core.Utilities;
using Motive.UI;
using Motive.Unity.Models;
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using Motive.Unity.World;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive.Unity.AR
{
    public class ARMarkerObject
    {
        public string MarkerIdentifier { get; private set; }
        public GameObject GameObject { get; private set; }

        public ARMarkerObject(string markerIdentifier, GameObject gameObject)
        {
            this.MarkerIdentifier = markerIdentifier;
            this.GameObject = gameObject;
        }
    }
    public class ARMarkerTrackingEventArgs : EventArgs
    {
        public ARMarkerObject MarkerObject { get; private set; }

        public ARMarkerTrackingEventArgs(string markerIdentifier, GameObject gameObject)
        {
            MarkerObject = new ARMarkerObject(markerIdentifier, gameObject);
        }
    }

    public interface IARMarkerAdapter
    {
        event EventHandler<ARMarkerTrackingEventArgs> StartedTracking;
        event EventHandler<ARMarkerTrackingEventArgs> LostTracking;

        void Initialize();
        void Activate();
        void Deactivate();

        void RegisterMarker(IVisualMarker marker);
    }

    /// <summary>
    /// Manages marker-based AR.
    /// </summary>
    public class ARMarkerManager : MonoBehaviour, IARWorldAdapter<VisualMarkerWorldObject>
    {
        public bool IsActive { get; private set; }

        /// <summary>
        /// Implementation-specific marker adapter (Vuforia, ARKit, ARCore, etc.)
        /// </summary>
        public ARMarkerAdapterBase Adapter;

        public ARMarkerTrackingConditionMonitor TrackingConditionMonitor { get; private set; }

        public GameObject InitializingPane;

        public UnityEvent Activated;
        public UnityEvent Deactivated;

        public bool IsInitialized { get; private set; }

        public bool ActivateOnWake;
        
        public VisualMarkerObject MarkerImageObject;
        public VisualMarkerObject MarkerVideoObject;
        public VisualMarkerObject MarkerAudioObject;
        public VisualMarkerObject MarkerAssetObject;

        public Vector3 ItemScale = Vector3.one;

        static ARMarkerManager g_instance;

        public static ARMarkerManager Instance
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
            public IVisualMarker Marker;
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

        /// <summary>
        /// Represents an activated media or asset. Marker Objects are the
        /// markers/objects that this activated type is attached to.
        /// </summary>
        class ActiveObject
        {
            public Dictionary<GameObject, VisualMarkerWorldObject> TrackingMarkerObjects;
            public SetDictionary<string, VisualMarkerWorldObject> HoldingMarkerObjects;

            public HashSet<string> PendingLoads;

            public ActiveObject()
            {
                PendingLoads = new HashSet<string>();
                TrackingMarkerObjects = new Dictionary<GameObject, VisualMarkerWorldObject>();
                HoldingMarkerObjects = new SetDictionary<string, VisualMarkerWorldObject>();
            }
        }

        // Marker objects for a given identifier
        SetDictionary<string, ARMarkerObject> m_trackingMarkerObjects;
        // World objects spawned on an AR marker
        SetDictionary<ARMarkerObject, VisualMarkerObject> m_spawnedObjects;
        List<ARMarkerObject> m_trackingHold;

        // Media and assets indexed by marker identifier
        SetDictionary<string, MarkerMedia> m_activeResourceMedia;
        SetDictionary<string, MarkerAsset> m_activeResourceAssets;
        
        // Tracks all active AR objects for a particular media/asset
        Dictionary<string, ActiveObject> m_activeResourceObjects;

        Logger m_logger;

        protected void Awake()
        {
            m_logger = new Logger(this);

            g_instance = this;

            m_spawnedObjects = new SetDictionary<ARMarkerObject, VisualMarkerObject>();
            m_trackingMarkerObjects = new SetDictionary<string, ARMarkerObject>();
            m_trackingHold = new List<ARMarkerObject>();

            m_activeResourceMedia = new SetDictionary<string, MarkerMedia>();
            m_activeResourceAssets = new SetDictionary<string, MarkerAsset>();
            m_activeResourceObjects = new Dictionary<string, ActiveObject>();

            TrackingConditionMonitor = new ARMarkerTrackingConditionMonitor();

            if (Activated == null) Activated = new UnityEvent();
            if (Deactivated == null) Deactivated = new UnityEvent();
        }

        public void Initialize()
        {
            // If the adapter isn't set, try to find it on the ARAdapter of ARWorld
            if (!Adapter)
            {
                Adapter = ARWorld.Instance.ARAdapter.GetComponent<ARMarkerAdapterBase>();
            }

            if (Adapter)
            {
                Adapter.StartedTracking += (sender, args) =>
                {
                    StartTracking(args.MarkerObject);
                };

                Adapter.LostTracking += (sender, args) =>
                {
                    StopTracking(args.MarkerObject);
                };

                Adapter.Initialize();

                if (ActivateOnWake)
                {
                    Activate();
                }
            }
        }

        public void Activate()
        {
            if (Adapter)
            {
                IsActive = true;

                m_logger.Debug("Activate IsInitialized={0}", IsInitialized);

                if (InitializingPane)
                {
                    InitializingPane.SetActive(true);
                }

                Adapter.Activate();
            }
        }

        IEnumerable<ActiveObject> GetActiveObjects(string markerIdent)
        {
            var activeMedias = m_activeResourceMedia[markerIdent];

            if (activeMedias != null)
            {
                foreach (var media in activeMedias)
                {
                    if (m_activeResourceObjects.ContainsKey(media.ObjectId))
                    {
                        yield return m_activeResourceObjects[media.ObjectId];
                    }
                }
            }

            var activeAssets = m_activeResourceAssets[markerIdent];

            if (activeAssets != null)
            {
                foreach (var asset in activeAssets)
                {
                    if (m_activeResourceObjects.ContainsKey(asset.ObjectId))
                    {
                        yield return m_activeResourceObjects[asset.ObjectId];
                    }
                }
            }
        }

        IEnumerable<GameObject> GetActiveGameObjects()
        {
            foreach (var kv in m_trackingMarkerObjects)
            {
                foreach (var obj in GetActiveObjects(kv.Key))
                {
                    foreach (var go in obj.TrackingMarkerObjects.Values)
                    {
                        yield return go.GameObject;
                    }
                }
            }
        }

        public void Deactivate()
        {
            IsActive = false;

            m_logger.Debug("Deactivate: IsInitialized = {0}", IsInitialized);

            if (IsInitialized)
            {
                var handlers = m_trackingMarkerObjects.Values.ToArray();

                foreach (var handler in handlers)
                {
                    StopTracking(handler);
                    m_trackingHold.Add(handler);
                }
            }

            if (Adapter)
            {
                Adapter.Deactivate();
            }

            if (Deactivated != null)
            {
                Deactivated.Invoke();
            }

            TrackingConditionMonitor.TrackingMarkersUpdated();
        }
        
        /// <summary>
        /// Checks if the given marker is tracking
        /// </summary>
        /// <param name="marker"></param>
        /// <returns></returns>
        public bool IsTracking(IVisualMarker marker)
        {
            if (!CheckAdapter())
            {
                return false;
            }

            m_logger.Debug("Is tracking {0}, IsActive={1}", marker.GetIdentifier(), IsActive);

            Adapter.RegisterMarker(marker);

            if (IsActive)
            {
                return m_trackingMarkerObjects.GetCount(marker.GetIdentifier()) > 0;
            }

            return false;
        }

        bool DoesObjectExist(string markerId, string objectId)
        {
            ActiveObject activeObject = null;

            if (m_activeResourceObjects.TryGetValue(objectId, out activeObject))
            {
                // TODO : return activeObject.MarkerObjects.ContainsKey(markerId);
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

        VisualMarkerWorldObject CreateWorldObject(ARMarkerObject markerObj, VisualMarkerObject spawnedItem, GameObject animationTarget, ResourceActivationContext context = null)
        {
            var worldObj = new VisualMarkerWorldObject
            {
                GameObject = spawnedItem.gameObject
            };

            spawnedItem.WorldObject = worldObj;

            ARWorld.Instance.AddWorldObject(worldObj, context);
            WorldObjectManager.Instance.AddWorldObject(context, spawnedItem.gameObject, animationTarget, spawnedItem.LayoutObject);

            m_spawnedObjects.Add(markerObj, spawnedItem);

            return worldObj;
        }

        void AttachAssetObject(MarkerAsset asset, ARMarkerObject markerObj)
        {
            var activeObject = GetOrCreateActiveObject(asset.ObjectId);

            var worldObj = activeObject.HoldingMarkerObjects.GetFirstOrDefault(markerObj.MarkerIdentifier);

            if (worldObj != null)
            {
                activeObject.HoldingMarkerObjects.Remove(markerObj.MarkerIdentifier, worldObj);
                activeObject.TrackingMarkerObjects.Add(markerObj.GameObject, worldObj);

                worldObj.GameObject.transform.SetParent(markerObj.GameObject.transform, false);
                worldObj.GameObject.transform.localPosition = Vector3.zero;
                worldObj.GameObject.transform.localScale = ItemScale;
                worldObj.GameObject.transform.localRotation = Quaternion.identity;

                if (asset.OnOpen != null)
                {
                    asset.OnOpen();
                }

                worldObj.GameObject.SetActive(true);
            }
            else
            {
                if (activeObject.PendingLoads.Contains(asset.Marker.GetIdentifier()))
                {
                    return;
                }

                activeObject.PendingLoads.Add(asset.Marker.GetIdentifier());

                AssetLoader.LoadAsset<GameObject>(asset.AssetInstance.Asset, (gameObj) =>
                {
                    activeObject.PendingLoads.Remove(asset.Marker.GetIdentifier());

                    // parent object could have been destroyed
                    if (gameObj && markerObj.GameObject)
                    {
                        var assetObj = Instantiate(MarkerAssetObject);

                        worldObj = CreateWorldObject(markerObj, assetObj, gameObj, asset.ActivationContext);

                        worldObj.Clicked += (sender, args) =>
                        {
                            if (asset.OnSelect != null)
                            {
                                asset.OnSelect();
                            }
                        };

                        gameObj.transform.SetParent(assetObj.LayoutObject.transform, false);
                        gameObj.transform.localPosition = Vector3.zero;

                        assetObj.transform.SetParent(markerObj.GameObject.transform, false);
                        assetObj.transform.localPosition = Vector3.zero;
                        assetObj.transform.localScale = ItemScale;
                        assetObj.transform.localRotation = Quaternion.identity;

                        if (asset.AssetInstance.Layout != null)
                        {
                            LayoutHelper.Apply(assetObj.LayoutObject.transform, asset.AssetInstance.Layout);
                        }

                        var collider = gameObj.GetComponent<Collider>();

                        if (!collider)
                        {
                            gameObj.AddComponent<SphereCollider>();
                        }

                        activeObject.TrackingMarkerObjects.Add(markerObj.GameObject, worldObj);

                        if (asset.OnOpen != null)
                        {
                            asset.OnOpen();
                        }
                    }
                    else
                    {
                        if (asset.MediaUrl != null)
                        {
                            AttachMediaObject(asset, markerObj);
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
            ARMarkerObject markerObj)
        {
            var activeObject = GetOrCreateActiveObject(media.ObjectId);

            VisualMarkerObject mediaObj = null;

            Action onReady = null;
            
            var worldObj = activeObject.HoldingMarkerObjects.GetFirstOrDefault(markerObj.MarkerIdentifier);

            if (worldObj != null)
            {
                activeObject.HoldingMarkerObjects.Remove(markerObj.MarkerIdentifier, worldObj);
                activeObject.TrackingMarkerObjects.Add(markerObj.GameObject, worldObj);

                mediaObj = worldObj.GameObject.GetComponent<VisualMarkerObject>();

                switch (media.MediaType)
                {
                    case Motive.Core.Media.MediaType.Audio:
                        onReady = () =>
                            {
                                var player = mediaObj.GetComponentInChildren<AudioSubpanel>();

                                player.Play(media.OnClose);
                            };

                        break;
                    case Motive.Core.Media.MediaType.Video:
                        onReady = () =>
                            {
                                var player = mediaObj.GetComponentInChildren<VideoSubpanel>();

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
                            mediaObj = Instantiate(MarkerAudioObject);

                            onReady = () =>
                                {
                                    var player = mediaObj.GetComponentInChildren<AudioSubpanel>();

                                    player.Play(media.MediaUrl, media.OnClose);
                                };

                            break;
                        }
                    case Motive.Core.Media.MediaType.Image:
                        {
                            mediaObj = Instantiate(MarkerImageObject);

                            onReady = () =>
                            {
                                ThreadHelper.Instance.StartCoroutine(
                                    ImageLoader.LoadImage(media.MediaUrl, mediaObj.RenderObject));
                            };

                            break;
                        }
                    case Motive.Core.Media.MediaType.Video:
                        {
                            mediaObj = Instantiate(MarkerVideoObject);

                            var renderer = mediaObj.RenderObject.GetComponent<Renderer>();

                            renderer.enabled = false;

                            onReady = () =>
                                {                       
                                    var player = mediaObj.GetComponentInChildren<VideoSubpanel>();

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

                if (mediaObj)
                {
                    if (media.MediaLayout != null)
                    {
                        LayoutHelper.Apply(mediaObj.LayoutObject.transform, media.MediaLayout);
                    }

                    if (media.Color != null)
                    {
                        var renderer = mediaObj.RenderObject.GetComponent<Renderer>();

                        if (renderer)
                        {
                            renderer.material.color = ColorHelper.ToUnityColor(media.Color);
                        }
                    }

                    worldObj = CreateWorldObject(markerObj, mediaObj, mediaObj.RenderObject, media.ActivationContext);

                    activeObject.TrackingMarkerObjects[markerObj.GameObject] = worldObj;

                    worldObj.Clicked += (sender, args) =>
                    {
                        if (media.OnSelect != null)
                        {
                            media.OnSelect();
                        }
                    };
                }
            }

            if (mediaObj)
            {
                mediaObj.transform.SetParent(markerObj.GameObject.transform, false);
                mediaObj.transform.localScale = ItemScale;
                mediaObj.transform.localPosition = Vector3.zero;
                mediaObj.transform.localRotation = Quaternion.identity;

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
        
        public void RemoveResourceObjects(string resourceId)
        {
            ActiveObject activeObj = null;

            // Indexed by media/asset instance id
            if (m_activeResourceObjects.TryGetValue(resourceId, out activeObj))
            {
                foreach (var obj in activeObj.TrackingMarkerObjects.Values)
                {
                    ARWorld.Instance.RemoveWorldObject(obj);
                }

                foreach (var obj in activeObj.HoldingMarkerObjects.Values)
                {
                    ARWorld.Instance.RemoveWorldObject(obj);
                }

                m_activeResourceObjects.Remove(resourceId);
            }

            // Indexed by marker ident
            var removeMedias = m_activeResourceMedia.Values.Where(v => v.ObjectId == resourceId).ToArray();

            foreach (var rm in removeMedias)
            {
                m_activeResourceMedia.Remove(rm.Marker.GetIdentifier(), rm);
            }

            // Indexed by marker ident
            var removeAssets = m_activeResourceAssets.Values.Where(v => v.ObjectId == resourceId).ToArray();

            foreach (var ra in removeAssets)
            {
                m_activeResourceAssets.Remove(ra.Marker.GetIdentifier(), ra);
            }

            WorldObjectManager.Instance.RemoveWorldObjects(resourceId);
        }

        void AddMarkerMedia(MarkerMedia markerMedia)
        {
            if (markerMedia.MediaUrl == null)
            {
                return;
            }

            m_activeResourceMedia.Add(markerMedia.Marker.GetIdentifier(), markerMedia);

            var objs = m_trackingMarkerObjects[markerMedia.Marker.GetIdentifier()];

            Adapter.RegisterMarker(markerMedia.Marker);

            if (objs != null)
            {
                foreach (var obj in objs)
                {
                    AttachMediaObject(
                        markerMedia,
                        obj);
                }
            }
        }

        bool CheckAdapter()
        {
            if (Adapter == null)
            {
                SystemErrorHandler.Instance.ReportError("Image Tracking is not supported by this app.");

                return false;
            }

            return true;
        }

        public void Add3DAsset(ResourceActivationContext ctxt, IVisualMarker marker, string mediaId, AssetInstance assetInstance, string fallbackImageUrl = null, Layout layout = null, Action onOpen = null, Action onSelect = null)
        {
            if (!CheckAdapter())
            {
                return;
            }

            if (DoesObjectExist(marker.GetIdentifier(), mediaId))
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

            Add3DAsset(markerAsset);
        }

        public void AddMarkerImage(ResourceActivationContext ctxt, IVisualMarker marker, string mediaId, string url, Layout layout = null, Action onOpen = null, Action onSelect = null)
        {
            if (!CheckAdapter())
            {
                return;
            }

            if (DoesObjectExist(marker.GetIdentifier(), mediaId))
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

        public void AddMarkerMedia(IVisualMarker marker, string mediaId, MediaElement mediaElement, Action onOpen = null, Action onSelect = null, Action onClose = null)
        {
            if (!CheckAdapter())
            {
                return;
            }

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
            if (!CheckAdapter())
            {
                return;
            }

            if (resource.MediaElement == null ||
                resource.MediaElement.MediaUrl == null ||
                resource.Markers == null)
            {
                return;
            }

            foreach (var marker in resource.Markers.OfType<IVisualMarker>())
            {
                if (marker != null)
                {
                    var media = new MarkerMedia
                        {   
                            ActivationContext = context,
                            Marker = marker,
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

            foreach (var marker in resource.Markers.OfType<IVisualMarker>())
            {
                if (marker != null)
                {
                    m_activeResourceMedia.RemoveWhere(marker.GetIdentifier(), c => c.ObjectId == context.InstanceId);
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

            foreach (var marker in resource.Markers.OfType<IVisualMarker>())
            {
                if (marker != null)
                {
                    m_activeResourceAssets.RemoveWhere(marker.GetIdentifier(), c => c.ObjectId == context.InstanceId);
                }
            }
            
            RemoveResourceObjects(context.InstanceId);
        }

        void Add3DAsset(MarkerAsset markerAsset)
        {
            Adapter.RegisterMarker(markerAsset.Marker);

            var objs = m_trackingMarkerObjects[markerAsset.Marker.GetIdentifier()];

            m_activeResourceAssets.Add(markerAsset.Marker.GetIdentifier(), markerAsset);

            if (objs != null && objs.Count() > 0)
            {
                foreach (var obj in objs)
                {
                    //obj.ExtendedTracking = markerAsset.Marker.UseExtendedTracking;

                    AttachAssetObject(markerAsset, obj);
                }
            }
            else
            {
                // Cache the asset if supported
                AssetLoader.PreloadAsset(markerAsset.AssetInstance.Asset);
            }
        }

        public void Add3DAsset(IVisualMarker marker, string objectId, AssetInstance assetInstance, MediaElement fallbackImage)
        {
            if (!CheckAdapter())
            {
                return;
            }

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

            Add3DAsset(markerAsset);
        }

        internal void Add3DAsset(ResourceActivationContext context, VisualMarker3DAsset resource)
        {
            if (!CheckAdapter())
            {
                return;
            }

            if (resource.AssetInstance == null ||
                resource.Markers == null)
            {
                return;
            }

            foreach (var marker in resource.Markers)
            {
                var vumark = marker as IVisualMarker;

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

                    Add3DAsset(asset);
                }
            }
        }

        internal void StopTracking(ARMarkerObject markerObj)
        {
            if (m_trackingHold.Contains(markerObj))
            {
                m_trackingHold.Remove(markerObj);

                return;
            }

            // Pause all audio/video players
            var audioPlayers = markerObj.GameObject.GetComponentsInChildren<AudioSubpanel>();
            var videoPlayers = markerObj.GameObject.GetComponentsInChildren<VideoSubpanel>();

            foreach (var ap in audioPlayers)
            {
                ap.Pause();
            }

            foreach (var vp in videoPlayers)
            {
                vp.Pause();
            }

            List<GameObject> objs = new List<GameObject>();

            for (int i = 0; i < markerObj.GameObject.transform.childCount; i++)
            {
                var t = markerObj.GameObject.transform.GetChild(i);
                objs.Add(t.gameObject);
            }

            foreach (var obj in objs)
            {
                obj.transform.SetParent(null);
                obj.SetActive(false);
            }

            var markerIdent = markerObj.MarkerIdentifier;

            foreach (var activeObj in GetActiveObjects(markerIdent))
            {
                VisualMarkerWorldObject worldObj = null;

                if (activeObj.TrackingMarkerObjects.TryGetValue(markerObj.GameObject, out worldObj))
                {
                    activeObj.TrackingMarkerObjects.Remove(markerObj.GameObject);
                    activeObj.HoldingMarkerObjects.Add(markerIdent, worldObj);
                }
            }

            if (markerIdent != null)
            {
                m_logger.Debug("stop tracking {0}", markerIdent);

                m_trackingMarkerObjects.RemoveWhere(markerIdent, m => m.GameObject == markerObj.GameObject);
            }

            TrackingConditionMonitor.TrackingMarkersUpdated();
        }

        internal void StartTracking(ARMarkerObject markerObj)
        {
            var markerIdent = markerObj.MarkerIdentifier;

            m_logger.Debug("start tracking {0}", markerIdent);

            m_trackingMarkerObjects.Add(markerIdent, markerObj);

            // Find all media items for the given marker
            var media = m_activeResourceMedia[markerIdent];

            m_logger.Debug("found {0} media items for {1}", media == null ? 0 : media.Count(), markerIdent);

            if (media != null)
            {
                foreach (var c in media)
                {
                    // Attach each media item to the marker
                    AttachMediaObject(c, markerObj);
                }
            }

            var assets = m_activeResourceAssets[markerIdent];

            if (assets != null)
            {
                foreach (var a in assets)
                {
                    AttachAssetObject(a, markerObj);
                }
            }

            TrackingConditionMonitor.TrackingMarkersUpdated();
        }

        public Camera GetCameraForObject(VisualMarkerWorldObject worldObj)
        {
            if (Adapter)
            {
                return Adapter.WorldCamera;
            }

            return null;
        }

        public void RemoveWorldObject(VisualMarkerWorldObject worldObject)
        {
            //throw new NotImplementedException();
        }

        public void AddWorldObject(VisualMarkerWorldObject worldObject)
        {
            //throw new NotImplementedException();
        }

        public double GetDistance(VisualMarkerWorldObject worldObject)
        {
            return 0;
            //throw new NotImplementedException();
        }
    }
}
