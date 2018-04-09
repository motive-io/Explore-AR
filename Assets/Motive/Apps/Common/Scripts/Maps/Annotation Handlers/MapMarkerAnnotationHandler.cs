// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Core.Utilities;
using Motive.UI;
using Motive.Unity.Maps;
using Motive.Unity.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMarkerAnnotationHandler : SingletonSimpleMapAnnotationHandler<MapMarkerAnnotationHandler,MapAnnotation<LocationMarker>>
{
    private SetDictionary<string, MapAnnotation<LocationMarker>> m_markerAnnotations;

    public CustomImageAnnotation CustomImageAnnotation;

    protected override void Awake()
    {
        m_markerAnnotations = new SetDictionary<string, MapAnnotation<LocationMarker>>();

        base.Awake();
    }

    public AnnotationGameObject CreateMarkerAnnotation(MediaElement marker)
    {
        CustomImageAnnotation annotationObj = Instantiate(CustomImageAnnotation);

        annotationObj.LoadMediaElement(marker);

        return annotationObj;
    }

    public virtual AnnotationGameObject GetDefaultAnnotationObject(MapView sender, IAnnotation annotation)
    {
        return Instantiate<AnnotationGameObject>(AnnotationPrefab);
    }

    public override AnnotationGameObject GetObjectForAnnotation(MapAnnotation<LocationMarker> annotation)
    {
        if (annotation.Marker != null)
        {
            return CreateMarkerAnnotation(annotation.Marker);
        }
        else if (annotation.AssetInstance != null)
        {
            var parent = new GameObject("3D Marker");
            var annotationObj = parent.AddComponent<AnnotationGameObject>();

            UnityAssetLoader.LoadAsset(annotation.AssetInstance, parent);

            return annotationObj;
        }

        return base.GetObjectForAnnotation(annotation);
    }

    internal void AddLocationMarker(string instanceId, LocationMarker marker)
    {
        if (marker.Locations != null)
        {
            foreach (var l in marker.Locations)
            {
                var ann = new MapAnnotation<LocationMarker>(l, marker);

                ann.Marker = marker.Marker;

                m_markerAnnotations.Add(instanceId, ann);

                AddAnnotation(ann);
            }
        }
    }

    internal void RemoveLocationMarker(string instanceId)
    {
        var annotations = m_markerAnnotations[instanceId];
        m_markerAnnotations.RemoveAll(instanceId);

        if (annotations != null)
        {
            foreach (var ann in annotations)
            {
                RemoveAnnotation(ann);
            }
        }
    }

    internal void AddLocation3DAsset(Location3DAsset resource)
    {
        if (resource.Locations != null)
        {
            foreach (var l in resource.Locations)
            {
                var ann = new MapAnnotation<LocationMarker>(l);

                ann.AssetInstance = resource.AssetInstance;

                m_markerAnnotations.Add(resource.Id, ann);

                AddAnnotation(ann);
            }
        }
    }

    internal void RemoveLocation3DAsset(Location3DAsset resource)
    {
        var annotations = m_markerAnnotations[resource.Id];
        m_markerAnnotations.RemoveAll(resource.Id);

        if (annotations != null)
        {
            foreach (var ann in annotations)
            {
                RemoveAnnotation(ann);
            }
        }
    }
}
