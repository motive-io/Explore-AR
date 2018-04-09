// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.AR.LocationServices;
using Motive.AR.Scripting;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using Motive.Core.Utilities;
using System.Linq;
using Motive.Core.Diagnostics;
using Motive.UI;
using Motive.Unity.UI;
using Motive.AR.Models;
using Motive.UI.Framework;
using UnityEngine.Events;
using Motive.Core.Models;

namespace Motive.Unity.Maps
{
    public class MapControllerEventArgs : EventArgs
    {
        public Coordinates Coordinates { get; private set; }

        public MapControllerEventArgs(Coordinates coords)
        {
            Coordinates = coords;
        }
    }

    public class MapControllerEvent : UnityEvent<MapControllerEventArgs>
    {
    }

    /// <summary>
    /// The map controller interprets user input and sends it to the MapView and
    /// manages the annotations on the map.
    /// </summary>
    public class MapController : SingletonComponent<MapController>, IMapViewDelegate
    {
        public Camera MapCamera;
        public GameObject MapSurface;

		public MapControllerEvent OnHold;
		public MapControllerEvent Ready;

        private SetDictionary<string, MapAnnotation> m_locationAnnotations;

        private Dictionary<string, MapAnnotation> m_searchLocations;
        private Dictionary<string, MediaElement> m_locationTypeMarkers;
        private Dictionary<string, MediaElement> m_storyTagMarkers;

        public AnnotationGameObject User;

        public MapView MapView;

        public MapInput MapInput;
        public bool DeselectAnnotationOnMapInput;

        public bool LockToUser;

        public float ZoomLevel = 15;
        public float PanSpeed = 0.1f;

        public Vector3 DefaultAnnotationElevation = new Vector3(0, .1f, 0);
        public Vector3 SelectedAnnotationElevation = new Vector3(0, .1f, 0);
        public Vector3 MapX = new Vector3(10f, 0, 0);
        public Vector3 MapY = new Vector3(0, 0, 10f);

        public virtual float DefaultZoomLevel
        {
            get
            {
                return ZoomLevel;
            }
        }

        public float MinZoom = 2;
        public float MaxZoom = 20;

        private bool m_firstUpdateComplete;
        //private double m_startMapZoom;
        private Coordinates m_startCoordinates;

        private UserAnnotation m_userAnnotation;

        // Pan/zoom
        private Coordinates m_panToCoordinates;
        private double? m_panToZoom;
        private float? m_panDuration;
        private Action m_onPanComplete;
        private float m_panStartTime;
        private double m_panStartZoom;
        private Coordinates m_panStartCoordinates;
        
        private IAnnotation m_lockAnnotation;

        private HashSet<MapAnnotation> m_toAdd;
        private HashSet<MapAnnotation> m_toRemove;

        public MapAnnotation SelectedAnnotation { get; private set; }

        Vector3 m_mapScreenTopRight;
        Vector3 m_mapScreenBottomLeft;
        float m_mapPixWidth;

        public Camera GetCamera()
        {
            return MapCamera ? MapCamera : Camera.main;
        }

        void SetScreenBounds()
        {
            Action<Bounds> setBounds = (bounds) =>
            {
                Vector3 origin = GetCamera().WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, 0f));
                Vector3 extent = GetCamera().WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, 0f));

                m_mapPixWidth = Mathf.Abs(extent.x - origin.x);
            };

            if (MapSurface)
            {
                var mesh = MapSurface.GetComponent<Mesh>();

                if (mesh)
                {
                    setBounds(mesh.bounds);
                    return;
                }

                var renderer = MapSurface.GetComponent<Renderer>();

                if (renderer)
                {
                    setBounds(renderer.bounds);
                    return;
                }

                var collider = MapSurface.GetComponent<Collider>();

                if (collider)
                {
                    setBounds(collider.bounds);
                    return;
                }
            }

            m_mapPixWidth = Screen.width;
        }

        protected override void Awake()
        {
            base.Awake();

            m_searchLocations = new Dictionary<string, MapAnnotation>();
            m_storyTagMarkers = new Dictionary<string, MediaElement>();
            m_locationTypeMarkers = new Dictionary<string, MediaElement>();
            m_locationAnnotations = new SetDictionary<string, MapAnnotation>();

            m_toAdd = new HashSet<MapAnnotation>();
            m_toRemove = new HashSet<MapAnnotation>();

			if (OnHold == null)
			{
				OnHold = new MapControllerEvent();
			}

			if (Ready == null)
			{
				Ready = new MapControllerEvent();
			}

            SetScreenBounds();
        }

        public virtual Coordinates GetCoordinatesForScreenPosition(Vector2 position)
        {
            var ray = Camera.main.ScreenPointToRay(position);

            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                // Todo: which game object represents the actual map?
                if (hitInfo.collider.gameObject == MapSurface)
                {
                    var texPt = MapSurface.transform.InverseTransformPoint(hitInfo.point);

                    var x = Vector3.Dot(texPt, MapX.normalized);
                    var y = Vector3.Dot(texPt, MapY.normalized);

                    var hitPoint = new Vector3(x, y);

                    var coords = GetCoordinatesForPosition(hitPoint);

                    return coords;
                }
            }

            return null;
        }

        void OnInputHold(MapInputEventArgs e)
        {
            var coords = GetCoordinatesForScreenPosition(e.TouchPosition);

            if (coords != null)
            {
                OnHold.Invoke(new MapControllerEventArgs(coords));
            }
        }

        void OnDoubleClick(MapInputEventArgs e)
        {
            var coords = GetCoordinatesForScreenPosition(e.TouchPosition);

            if (coords != null)
            {
                CenterMap(coords, MapView.Zoom + MapInput.DoubleClickZoomDelta, 0.33f);
            }
        }

        public void RefreshUserAnnotation()
        {
            MapView.RefreshAnnotation(m_userAnnotation);
        }

        public void LockOnUser(bool lockOn = true)
        {
            LockOnAnnotation(lockOn ? m_userAnnotation : null);
        }

        public void LockOnAnnotation(IAnnotation ann)
        {
            m_lockAnnotation = ann;
        }
        
        protected override void Start()
        {
            var mesh = MapSurface.GetComponent<MeshRenderer>();

            var pos = MapSurface.transform.position;

            var topRight = new Vector2(pos.x + mesh.bounds.max.x, pos.y + mesh.bounds.max.y);
            var bottomLeft = new Vector2(pos.x + mesh.bounds.min.x, pos.y + mesh.bounds.min.y);

            m_mapScreenTopRight = Camera.main.WorldToScreenPoint(topRight);
            m_mapScreenBottomLeft = Camera.main.WorldToScreenPoint(bottomLeft);

            MapInput.OnHold.AddListener(OnInputHold);
            MapInput.OnDoubleClick.AddListener(OnDoubleClick);

            MapView.Delegate = this;
        }

        protected virtual void Update()
        {
            if (!m_firstUpdateComplete)
            {
                if (ForegroundPositionService.Instance.HasLocationData)
                {
					if (Ready != null)
					{
						Ready.Invoke(new MapControllerEventArgs(ForegroundPositionService.Instance.Position));
					}

                    m_userAnnotation = new UserAnnotation();
                    m_userAnnotation.Coordinates = ForegroundPositionService.Instance.Position;

                    if (LockToUser)
                    {
                        LockOnUser(true);
                    }

                    MapView.AddAnnotation(m_userAnnotation);

                    var startCoords = m_startCoordinates ?? ForegroundPositionService.Instance.Position;
                    SetMapRegion(startCoords, ZoomLevel);
                    //m_startMapZoom = ZoomLevel;

                    m_firstUpdateComplete = true;
                }
            }
            else
            {
                // Add/remove any annotations on the correct thread
                if (m_toAdd.Count > 0)
                {
                    lock (m_toAdd)
                    {
                        var toAdd = m_toAdd.ToArray();
                        m_toAdd.Clear();

                        foreach (var ann in toAdd)
                        {
                            MapView.AddAnnotation(ann);
                        }
                    }
                }

                if (m_toRemove.Count > 0)
                {
                    lock (m_toRemove)
                    {
                        var toRemove = m_toRemove.ToArray();
                        m_toRemove.Clear();

                        foreach (var ann in toRemove)
                        {
                            MapView.RemoveAnnotation(ann);
                        }
                    }
                }

                // The map view will not necessarily re-check the coordinates unless the
                // told directly.
                m_userAnnotation.UpdateCoordinates(ForegroundPositionService.Instance.Position, Time.deltaTime);
                MapView.UpdateAnnotation(m_userAnnotation);

                if (m_lockAnnotation != null)
                {
                    SetMapRegion(m_lockAnnotation.Coordinates, MapView.Zoom);

                    if (MapInput.PointerDown && DeselectAnnotationOnMapInput)
                    {
                        SelectAnnotation(null);
                    }
                }
                else
                {
                    if (MapInput.IsTranslating)
                    {
                        m_panToCoordinates = null;
                        m_panToZoom = null;

                        if (DeselectAnnotationOnMapInput)
                        {
                            SelectAnnotation(null);
                        }

                        if (MapInput.Translation.sqrMagnitude > 0)
                        {
                            var tx = -MapInput.Translation.x / (m_mapPixWidth);
                            var ty = MapInput.Translation.y / (m_mapPixWidth);

                            MapView.Translate(tx, ty);
                        }
                    }
                    else
                    {
                        //m_currTranslate = Vector2.zero;

                        if (m_panToCoordinates != null)
                        {
                            var zoom = MapView.Zoom == 0 ? ZoomLevel : MapView.Zoom;
                            Coordinates coordinates = null;

                            if (m_panDuration.HasValue)
                            {
                                // Use smoothstep for time-based transitions
                                var dt = Mathf.Min((Time.time - m_panStartTime) / m_panDuration.Value, 1f);

                                var delta = Mathf.SmoothStep(0, 1, dt);

                                coordinates = m_panStartCoordinates.Interpolate(m_panToCoordinates, delta);

                                if (m_panToZoom.HasValue)
                                {
                                    zoom = Mathf.SmoothStep((float)m_panStartZoom, (float)m_panToZoom, dt);
                                }
                            }
                            else
                            {
                                // Use exponential - good for general purpose as it takes longer over long
                                // distances
                                coordinates = MapView.CenterCoordinates.Approach(m_panToCoordinates, PanSpeed, Time.deltaTime);

                                if (m_panToZoom.HasValue)
                                {
                                    zoom = MathHelper.Approach(zoom, m_panToZoom.Value, PanSpeed, Time.deltaTime);
                                }
                            }

                            var dz = m_panToZoom.HasValue ? Math.Abs(zoom - m_panToZoom.Value) : 0;

                            SetMapRegion(coordinates, zoom);

                            if (coordinates.GetDistanceFrom(m_panToCoordinates) < 0.5 &&
                                dz < 0.1)
                            {
                                SetMapRegion(m_panToCoordinates, m_panToZoom ?? MapView.Zoom);

                                m_panToCoordinates = null;

                                if (m_onPanComplete != null)
                                {
                                    m_onPanComplete();
                                }
                            }
                        }
                    }
                }

                if (MapInput.IsPinching)
                {
                    SetMapRegion(MapView.CenterCoordinates, MapView.Zoom + MapInput.ZoomDelta);
                }
            }
        }

        private void SetMapRegion(Coordinates coordinates, double zoom)
        {
            MapView.SetRegion(coordinates, MathHelper.Clamp(zoom, MinZoom, MaxZoom));
        }

        public void RemoveAnnotation(MapAnnotation annotation)
        {
            lock (m_toAdd)
            {
                m_toAdd.Remove(annotation);
            }

            if (annotation == SelectedAnnotation)
            {
                SelectAnnotation(null);
            }

            m_locationAnnotations.Remove(annotation.Location.Id, annotation);

            lock (m_toRemove)
            {
                m_toRemove.Add(annotation);
            }
        }

        public void AddAnnotation(MapAnnotation annotation)
        {
            lock (m_toRemove)
            {
                m_toRemove.Remove(annotation);
            }

            m_locationAnnotations.Add(annotation.Location.Id, annotation);

            lock (m_toAdd)
            {
                m_toAdd.Add(annotation);
            }
        }

        public void CenterMap()
        {
            CenterMap(null);
        }

        public void CenterMap(bool setDefaultZoom)
        {
            CenterMap(null, setDefaultZoom ? DefaultZoomLevel : MapView.Zoom);
        }

        public virtual void CenterMap(
            Coordinates coordinates, 
            double? zoom = null, 
            float? duration = null, 
            Action onPanComplete = null)
        {
            MapInput.CancelPan();

            if (coordinates == null)
            {
                coordinates = ForegroundPositionService.Instance.Position;
            }

            m_panStartTime = Time.time;

            if (MapView != null)
            {
                m_panToCoordinates = coordinates;
                m_panToZoom = zoom.GetValueOrDefault(MapView.Zoom);
                m_panDuration = duration;
                m_onPanComplete = onPanComplete;
                m_panStartCoordinates = MapView.CenterCoordinates;
                m_panStartZoom = MapView.Zoom;
            }
            else
            {
                m_startCoordinates = coordinates;
                m_panToZoom = zoom;
            }
        }

        public void CenterMap(
            Coordinates coordinates,
            Action onPanComplete)
        {
            CenterMap(coordinates, null, null, onPanComplete);
        }

        protected virtual bool ShouldAddSearchLocationAnnotation(Location location)
        {
            return true;
        }

        protected void HandleLocationsUpdated(object sender, System.EventArgs e)
        {
            var oldSearchAnns = m_searchLocations;
            m_searchLocations = new Dictionary<string, MapAnnotation>();

            ThreadHelper.Instance.CallOnMainThread(() =>
            {
                foreach (var loc in LocationCache.Instance.Locations)
                {
                    if (!ShouldAddSearchLocationAnnotation(loc))
                    {
                        continue;
                    }

                    if (oldSearchAnns.ContainsKey(loc.Id))
                    {
                        // We already have this annotation,
                        // no need to re-add
                        var ann = oldSearchAnns[loc.Id];
                        oldSearchAnns.Remove(loc.Id);
                        m_searchLocations[loc.Id] = ann;
                    }
                    else
                    {
                        var ann = new MapAnnotation(loc);
                        m_searchLocations[loc.Id] = ann;

                        // When resolving markers, check location types first,
                        // then story tags.
                        if (loc.LocationTypes != null)
                        {
                            foreach (var t in loc.LocationTypes)
                            {
                                MediaElement marker = null;

                                if (m_locationTypeMarkers.TryGetValue(t, out marker))
                                {
                                    ann.Marker = marker;
                                    break;
                                }
                            }
                        }

                        if (ann.Marker == null && loc.StoryTags != null)
                        {
                            foreach (var s in loc.StoryTags)
                            {
                                MediaElement marker = null;

                                if (m_storyTagMarkers.TryGetValue(s, out marker))
                                {
                                    ann.Marker = marker;
                                    break;
                                }
                            }
                        }

                        AddAnnotation(ann);
                    }
                }

                // Any that are left in the old dictionary can be removed
                foreach (var ann in oldSearchAnns.Values)
                {
                    MapView.RemoveAnnotation(ann);
                }
            });
        }

        #region IMapViewDelegate
        public virtual System.Collections.Generic.IEnumerable<IAnnotation> GetAnnotationsForCluster(MapView sender, AnnotationCluster cluster)
        {
            throw new System.NotImplementedException();
        }

        public virtual ClusterId GetClusterIdForAnnotation(MapView sender, IAnnotation annotation, Motive.Core.Utilities.DoubleVector2 offset)
        {
            throw new System.NotImplementedException();
        }

        internal AnnotationGameObject Create3DAssetAnnotation(_3D.Models.AssetInstance assetInstance)
        {
            var parent = new GameObject("3D Marker");
            var annotationObj = parent.AddComponent<AnnotationGameObject>();

            UnityAssetLoader.LoadAsset(assetInstance, parent);

            return annotationObj;
        }

        public virtual AnnotationGameObject GetUserAnnotationObject(MapView sender, IAnnotation annotation)
        {
            return Instantiate<AnnotationGameObject>(User);
        }
        /*
        public virtual AnnotationGameObject GetSearchAnnotationObject(MapView sender, IAnnotation annotation)
        {
            return Instantiate<AnnotationGameObject>(DefaultAnnotation);
        }*/

        public virtual GameObject GetObjectForAnnotation(MapView sender, IAnnotation annotation)
        {
            if (annotation is UserAnnotation)
            {
                return GetUserAnnotationObject(sender, annotation).gameObject;
            }

            AnnotationGameObject annotationObj = null;

            var mapAnnotation = annotation as MapAnnotation;

            if (mapAnnotation.Delegate != null)
            {
                annotationObj = mapAnnotation.Delegate.GetObjectForAnnotation(mapAnnotation);
            }

            /*
            else
            {
                if (mapAnnotation.Marker != null)
                {
                    annotationObj = CreateMarkerAnnotation(mapAnnotation.Marker);
                }
                else if (mapAnnotation.AssetInstance != null)
                {
                    annotationObj = Create3DAssetAnnotation(mapAnnotation.AssetInstance);
                }
                else if (m_searchLocations.ContainsKey(mapAnnotation.Location.Id))
                {
                    annotationObj = GetSearchAnnotationObject(sender, mapAnnotation);
                }
                else
                {
                    annotationObj = GetDefaultAnnotationObject(sender, mapAnnotation);
                }
            }
            */

            if (annotationObj != null)
            {
                annotationObj.Annotation = mapAnnotation;
                return annotationObj.gameObject;
            }

            return null;
        }

        public virtual Coordinates GetCoordinatesForPosition(Vector3 position)
        {
            // This call reverses the call below, then maps back to coordinates.
            var x = (position.x + 5f) / 10f;
            var y = (position.y + 5f) / 10f;

            return MapView.TileDriver.GetCoordinatesFromOffset(x, y);
        }

        public virtual void ConfigureTransformForAnnotationObject(MapView sender, IAnnotation annotation, DoubleVector2 offset, Transform transform)
        {
            // The map view needs to know where the annotation lives on the texture.
            // We used a plane for the texture, which has dimensions of 10x10.
            // The offset in the call tells us where the annotation sits on the texture
            // surface, from extents 0,0 to 1,1. We need to convert these values to
            // a location on the plane itself.

            Vector3 pos =
                (float)offset.X * MapX - (MapX / 2) +
                (IsAnnotationSelected(annotation) ? SelectedAnnotationElevation : DefaultAnnotationElevation) +
                (float)offset.Y * MapY - (MapY / 2);
                //new
                //Vector3((float)offset.X * 10.0f - 5f, dy, (float)offset.Y * 10.0f - 5f);

            transform.localPosition = pos;
        }

        public virtual void RecycleAnnotationObject(MapView sender, GameObject annotationObject)
        {
            Destroy(annotationObject);
        }

        public virtual bool ShouldClusterAnnotation(MapView sender, IAnnotation annotation)
        {
            return false;
        }
        #endregion

        public bool IsAnnotationSelected(IAnnotation mapAnnotation)
        {
            return mapAnnotation == SelectedAnnotation;
        }

        public virtual void SelectAnnotation(MapAnnotation mapAnnotation)
        {
            var oldSelected = SelectedAnnotation;

            SelectedAnnotation = mapAnnotation;

            if (mapAnnotation != null &&
                mapAnnotation.Delegate != null)
            {
                mapAnnotation.Delegate.SelectAnnotation(mapAnnotation);
            }
            else
            {
                //ShowSelectedLocationPanel(mapAnnotation, DefaultSelectedLocationPanel);

                if (mapAnnotation != null)
                {
                    CenterMap(mapAnnotation.Coordinates);
                }
            }

            if (mapAnnotation != oldSelected &&
                oldSelected != null &&
                oldSelected.Delegate != null)
            {
                oldSelected.Delegate.DeselectAnnotation(oldSelected);
            }
        }

        public MapAnnotation GetAnnotationForLocation(Location location)
        {
            var anns = m_locationAnnotations[location.Id];

            if (anns != null)
            {
                return anns.FirstOrDefault();
            }

            return null;
        }

        public void SelectLocation(Location location)
        {
            var annotation = GetAnnotationForLocation(location);

            if (annotation != null)
            {
                SelectAnnotation(annotation);
            }
        }

        public void FocusAnnotation(MapAnnotation annotation, int range = 0)
        {
            var d1 = ForegroundPositionService.Instance.Position.GetDistanceFrom(annotation.Coordinates) * 3;

            // "magic number" : 150 is the closest we want to zoom
            var d = Math.Max(Math.Max(d1, range), 150);

            if (MapController.Instance.SelectedAnnotation == annotation)
            {
                MapController.Instance.PanZoomTo(annotation.Coordinates, new Vector { X = d, Y = d }, .6f);
            }
            else
            {
                MapController.Instance.PanZoomTo(annotation, new Vector { X = d, Y = d }, .6f);
            }
        }

        public void PanZoomTo(MapAnnotation ann, Motive.Core.Models.Vector span, float duration, Action onComplete = null)
        {
            PanZoomTo(ann.Coordinates, span, duration, () =>
                {
                    SelectAnnotation(ann);
                });
        }

        public void PanZoomTo(Coordinates coordinates, Motive.Core.Models.Vector span, float duration, Action onComplete = null)
        {
            // TODO: currently only works with 2D map
            double? targetZoom = null;

            var sw = m_mapScreenTopRight.x - m_mapScreenBottomLeft.x;
            var sh = m_mapScreenTopRight.y - m_mapScreenBottomLeft.y;

            var pw = Screen.width / sw;
            var ph = Screen.height / sh;

            var min = Math.Min(pw, ph);

            if (span != null)
            {
                targetZoom = MapView.TileDriver.GetZoomForDistanceSpan(span.X / min, span.Y / min);
            }

            CenterMap(coordinates, targetZoom, duration, onComplete);
        }
    }
}
