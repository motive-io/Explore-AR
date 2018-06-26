// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Motive.AR.LocationServices;
using Motive.Unity.Utilities;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// The game object displayed when an annotation is visible on a map.
    /// </summary>
    public class AnnotationGameObject : MonoBehaviour, IPointerClickHandler
    {
        public GameObject MarkerElement;

        public MapAnnotation Annotation { get; set; }
		public bool IsSelected { get; private set; }
        public ResizeWithMapZoom ResizeElement;
        public ResizeWithMapZoom RangeElement;
        public GameObject SelectResizeElement;
		public GameObject[] EnableWhenSelected;
		public GameObject[] EnableWhenNotSelected;
        public float SelectedScale = 2f;
        public AnnotationRangeDisplayMode RangeDisplayMode;
        public float Range;
        public bool MonitorRange;

        public Color InRangeColor;
        public Color OutOfRangeColor;

        private Vector3 m_scale;
        private Vector3 m_selectedScale;

        // Use this for initialization
        void Start()
        {
            if (SelectResizeElement)
            {
				m_scale = SelectResizeElement.transform.localScale;
                m_selectedScale = m_scale * SelectedScale;
            }

            if (ResizeElement)
            {
                ResizeElement.Range = Range;
            }

            if (RangeElement)
            {
                RangeElement.Range = Range;
            }

            if (RangeDisplayMode == AnnotationRangeDisplayMode.Never)
            {
                if (RangeElement)
                {
                    RangeElement.gameObject.SetActive(false);
                }
            }
            else if (RangeDisplayMode == AnnotationRangeDisplayMode.Always)
            {
                if (RangeElement)
                {
                    RangeElement.gameObject.SetActive(true);
                }
            }

			SetSelected(false);
        }

		public void SetSelected(bool selected)
		{
			IsSelected = selected;

			if (SelectResizeElement)
			{
				SelectResizeElement.transform.localScale = selected ? m_selectedScale : m_scale;
			}

            ObjectHelper.SetObjectsActive(EnableWhenSelected, selected);
            ObjectHelper.SetObjectsActive(EnableWhenNotSelected, !selected);

            if (RangeDisplayMode == AnnotationRangeDisplayMode.WhenSelected && RangeElement)
            {
                RangeElement.gameObject.SetActive(selected);
            }
		}

        // Update is called once per frame
        void Update()
        {
			if (MapController.Instance.SelectedAnnotation == Annotation && !IsSelected)
			{
				SetSelected(true);
			}
			else if (MapController.Instance.SelectedAnnotation != Annotation && IsSelected)
			{				
				SetSelected(false);
			}

            if (MonitorRange && RangeElement && RangeDisplayMode != AnnotationRangeDisplayMode.Never)
            {
                var renderer = RangeElement.GetComponent<SpriteRenderer>();

                if (renderer)
                {
                    var d = ForegroundPositionService.Instance.Position.GetDistanceFrom(Annotation.Coordinates);

                    renderer.color = (d < Range) ? InRangeColor : OutOfRangeColor;
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            MapController.Instance.SelectAnnotation(Annotation);
        }
    }
}