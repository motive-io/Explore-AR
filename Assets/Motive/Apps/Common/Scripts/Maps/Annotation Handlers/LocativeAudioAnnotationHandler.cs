// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.AR.Models;
using Motive.Unity.Maps;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocativeAudioAnnotation : MapAnnotation
{
    public LocativeAudioContent LocativeAudio { get; private set; }

    public LocativeAudioAnnotation(Location location, LocativeAudioContent locativeAudio)
        : base(location)
    {
        this.LocativeAudio = locativeAudio;
    }
}

public class LocativeAudioAnnotationHandler : SingletonSimpleMapAnnotationHandler<LocativeAudioAnnotationHandler>
{
    Dictionary<string, LocativeAudioContent> m_locativeAudioDict;

	public AnnotationGameObject _3DAnnotationPrefab;
	public AnnotationGameObject _2DAnnotationPrefab;

    public Color _3DInRangeColor;
    public Color _3DOutOfRangeColor;

    public Color _2DInRangeColor;
    public Color _2DOutOfRangeColor;

    protected override void Awake()
    {
        m_locativeAudioDict = new Dictionary<string, LocativeAudioContent>();

        base.Awake();
    }

	public override AnnotationGameObject GetPrefabForAnnotation (MapAnnotation annotation)
	{
		var ann = annotation as LocativeAudioAnnotation;

		bool is3d = ann.LocativeAudio.Is3D;

		var checkAnn = is3d ? _3DAnnotationPrefab : _2DAnnotationPrefab;

		return checkAnn != null ? checkAnn : AnnotationPrefab;
	}

    protected override void ConfigureAnnotationObject(MapAnnotation annotation, AnnotationGameObject obj)
    {
        var ann = annotation as LocativeAudioAnnotation;
        bool is3d = ann.LocativeAudio.Is3D;
        //var rangeImage = obj.RangeElement == null ? null : obj.RangeElement.GetComponent<SpriteRenderer>();

        if (ann.LocativeAudio.DistanceRange != null &&
            ann.LocativeAudio.DistanceRange.Max.HasValue)
        {
            obj.MonitorRange = true;
            obj.RangeDisplayMode = AnnotationRangeDisplayMode.Always;
            obj.Range = (float)ann.LocativeAudio.DistanceRange.Max.Value;
            obj.OutOfRangeColor = is3d ? _3DOutOfRangeColor : _2DOutOfRangeColor;
            obj.InRangeColor = is3d ? _3DInRangeColor : _2DInRangeColor;
        }
        else
        {
            obj.RangeDisplayMode = AnnotationRangeDisplayMode.Never;
        }
    }

    public void AddLocativeAudio(string instanceId, LocativeAudioContent locativeAudio)
    {
        if (locativeAudio.Locations != null)
        {
            m_locativeAudioDict[instanceId] = locativeAudio;

            foreach (var loc in locativeAudio.Locations)
            {
                AddAnnotation(new LocativeAudioAnnotation(loc, locativeAudio));
            }
        }
    }

    public void RemoveLocativeAudio(string instanceId)
    {
        LocativeAudioContent locativeAudio;

        if (m_locativeAudioDict.TryGetValue(instanceId, out locativeAudio))
        {
            if (locativeAudio.Locations != null)
            {
                foreach (var loc in locativeAudio.Locations)
                {
                    RemoveAnnotation(loc.Id);
                }
            }
        }
    }
}
