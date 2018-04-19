// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.UI.Framework;
using Motive.Unity.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Motive.Unity.Maps
{
    /// <summary>
    /// Base class for map annotation handlers.
    /// </summary>
    public class MapAnnotationHandler : MapAnnotationHandler<MapAnnotation>
    {
        protected void AddAnnotation(Location location)
        {
            AddAnnotation(location.Id, new MapAnnotation(location));
        }

        protected void AddAnnotation(string instanceId, Location location)
        {
            AddAnnotation(instanceId, new MapAnnotation(location));
        }
    }

    /// <summary>
    /// Convenience class for handling map annotations. Maintains a location -> annotation dictionary.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MapAnnotationHandler<T> :
        MapAnnotationHandlerBase,
        IMapAnnotationDelegate
        where T : MapAnnotation
    {
        protected Dictionary<string, T> m_annotations;

        public bool AnnotationsAreShowing = true;

        protected virtual void Awake()
        {
            m_annotations = new Dictionary<string, T>();
        }

		public virtual void Initialize() {}

        public override void HideAnnotations()
        {
            AnnotationsAreShowing = false;

            // We could be calling Hide before this component
            // has actually Start-ed.
            if (m_annotations != null)
            {
				lock (m_annotations)
				{
					foreach (var ann in m_annotations.Values)
					{
						MapController.Instance.RemoveAnnotation(ann);
					}
				}
            }
        }

        public override void ShowAnnotations()
        {
            AnnotationsAreShowing = true;

            if (m_annotations != null)
            {
				lock (m_annotations)
				{
					foreach (var ann in m_annotations.Values)
					{
						MapController.Instance.AddAnnotation(ann);
					}
				}
            }
        }

        protected void RemoveAnnotation(string annotationId, T annotation)
        {
            MapController.Instance.RemoveAnnotation(annotation);

			lock (m_annotations)
			{
				m_annotations.Remove(annotationId);
			}
        }

        protected void RemoveAnnotation(string annotationId)
        {
            var ann = GetAnnotation(annotationId);

            if (ann != null)
            {
                RemoveAnnotation(annotationId, ann);
            }
        }

        protected void RemoveAnnotation(T ann)
        {
            RemoveAnnotation(ann.Location.Id, ann);
        }

        protected void RemoveAnnotation(Location location)
        {
            RemoveAnnotation(location.Id);
        }

        protected void RemoveAllAnnotations()
        {
			KeyValuePair<string, T>[] annotations;

			lock (m_annotations)
			{
				annotations = m_annotations.ToArray();
			}

			foreach (var kv in annotations)
            {
                RemoveAnnotation(kv.Key, kv.Value);
            }
        }

		protected void AddAnnotation(string annotationId, T annotation)
        {
            if (annotation.Delegate == null)
            {
                annotation.Delegate = this;
            }

			lock (m_annotations)
			{
				T curr = null;

				if (m_annotations.TryGetValue(annotationId, out curr))
				{
					/*
					if (!replaceIfExists)
					{
						return;
					}*/

					MapController.Instance.RemoveAnnotation(curr);
				}

				m_annotations[annotationId] = annotation;

				if (AnnotationsAreShowing)
				{
					MapController.Instance.AddAnnotation(annotation);
				}
			}
        }

        protected void AddAnnotation(
            T annotation, 
            Func<MapAnnotation, AnnotationGameObject> getObjectForAnnotation = null,
            Action<MapAnnotation> selectAnnotation = null)
        {
            AddAnnotation(annotation.Location.Id, annotation, getObjectForAnnotation, selectAnnotation);
        }

        protected void AddAnnotation(
            string annotationId,
            T annotation, 
            Func<MapAnnotation, AnnotationGameObject> getObjectForAnnotation = null,
            Action<MapAnnotation> selectAnnotation = null)
        {
            if (annotation.Delegate == null && getObjectForAnnotation != null)
            {
                var del = new MapAnnotationDelegate
                {
                    OnGetObjectForAnnotation = getObjectForAnnotation ?? GetObjectForAnnotation,
                    OnSelect = selectAnnotation ?? SelectAnnotation
                };

                annotation.Delegate = del;
            }
            else
            {
                annotation.Delegate = this;
            }

			lock (m_annotations)
			{
				m_annotations[annotationId] = annotation;
			}

            if (AnnotationsAreShowing && MapController.Instance)
            {
                MapController.Instance.AddAnnotation(annotation);
            }
        }

        public T GetAnnotation(string annotationId)
        {
            T ann = null;

			lock (m_annotations)
			{
				m_annotations.TryGetValue(annotationId, out ann);
			}

            return ann;
        }

        protected virtual void Start()
        {
        }

        public virtual AnnotationGameObject GetPrefabForAnnotation(T annotation)
        {
            return null;
        }

        public virtual AnnotationGameObject GetPrefabForAnnotation(MapAnnotation annotation)
        {
            return GetPrefabForAnnotation((T)annotation);
        }

        public virtual AnnotationGameObject GetObjectForAnnotation(MapAnnotation annotation)
        {
            return null;
        }

        public virtual void SelectAnnotation(MapAnnotation annotation)
        {
        }

        public virtual void DeselectAnnotation(MapAnnotation annotation)
        {
        }
    }

    public abstract class MapAnnotationHandlerBase : MonoBehaviour
    {
        public abstract void HideAnnotations();

        public abstract void ShowAnnotations();
    }

    public class SingletonMapAnnotationHandler<T> :
        MapAnnotationHandler
        where T : SingletonMapAnnotationHandler<T>
    {
        static T sInstance = null;

        public static T Instance
        {
            get { return sInstance; }
        }

        protected override void Awake()
        {
            if (sInstance != null)
            {
                Debug.LogError("SingletonMapAnnotationHandler.Awake: error " + name + " already initialized");
            }

            sInstance = (T)this;

            base.Awake();
        }
    }
}
