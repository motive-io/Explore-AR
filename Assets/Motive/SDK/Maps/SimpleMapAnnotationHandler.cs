// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Maps;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Base class for handling map annotations with a selected location panel and one 
    /// game object prefab.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleMapAnnotationHandler<T> : MapAnnotationHandler<T>
        where T : MapAnnotation
    {
        public AnnotationGameObject AnnotationPrefab;
        public Panel SelectedLocationPanel;

        public AnnotationRangeDisplayMode RangeDisplayMode;
        public int Range;

        public bool CenterMapOnSelectedAnnotation = true;

        public virtual Panel GetSelectedLocationPanel()
        {
            return SelectedLocationPanel;
        }

        public override AnnotationGameObject GetPrefabForAnnotation(T annotation)
        {
            return AnnotationPrefab;
        }

        protected virtual void ConfigureAnnotationObjectRange(T annotation, AnnotationGameObject obj)
        {
            obj.RangeDisplayMode = RangeDisplayMode;

            switch (RangeDisplayMode)
            {
                case AnnotationRangeDisplayMode.Always:
                case AnnotationRangeDisplayMode.WhenSelected:
                    obj.Range = Range;
                    break;
            }
        }

        protected virtual void ConfigureAnnotationObject(T annotaion, AnnotationGameObject obj)
        {
            ConfigureAnnotationObjectRange(annotaion, obj);
        }

        public virtual AnnotationGameObject GetObjectForAnnotation(T annotation)
        {
            var prefab = GetPrefabForAnnotation(annotation);

            if (prefab)
            {
                var obj = Instantiate(prefab);
                obj.Annotation = annotation;

                ConfigureAnnotationObject(annotation, obj);

                return obj;
            }

            return null;
        }

        public override AnnotationGameObject GetObjectForAnnotation(MapAnnotation annotation)
        {
            return GetObjectForAnnotation(annotation as T);
        }

        public override void DeselectAnnotation(MapAnnotation annotation)
        {
            var panel = GetSelectedLocationPanel();

            if (panel)
            {
                SelectedLocationPanelHandler.Instance.HideSelectedLocationPanel(panel, annotation);
            }
        }

        public override void SelectAnnotation(MapAnnotation annotation)
        {
            var panel = GetSelectedLocationPanel();

            if (panel)
            {
                SelectedLocationPanelHandler.Instance.ShowSelectedLocationPanel(panel, annotation);

                ConfigureSelectedLocationPanel(panel, (T)annotation);
            }

            if (CenterMapOnSelectedAnnotation && annotation != null)
            {
                MapController.Instance.CenterMap(annotation.Coordinates);
            }
        }

        protected virtual void ConfigureSelectedLocationPanel(Panel panel, T annotation)
        {
        }
    }

    /// <summary>
    /// Simple annotation handler for MapAnnotaions.
    /// </summary>
    public class SimpleMapAnnotationHandler : SimpleMapAnnotationHandler<MapAnnotation>
    {
    }

}