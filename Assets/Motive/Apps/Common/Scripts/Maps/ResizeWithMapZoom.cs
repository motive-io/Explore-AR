// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Maps;
using UnityEngine;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Attach to a game object to resize it in step with the map.
    /// </summary>
    public class ResizeWithMapZoom : MonoBehaviour
    {
        double m_lastRange;
        double m_lastZoom;

        public Vector3 RangeUnitScale = Vector3.one;

        public double Range
        {
            get;
            set;
        }

        // Use this for initialization
        void Start()
        {
            m_lastRange = 0;
            m_lastZoom = 0;

            SetScale();
        }

        void SetScale()
        {
            if (MapController.Instance.MapView.TileDriver.Zoom != m_lastZoom ||
                m_lastRange != Range)
            {
                var viewScale = MapController.Instance.MapView.TileDriver.GetDistanceScale(MapController.Instance.MapView.CenterCoordinates, Range);

                //var renderer = MapController.Instance.MapView.MapTexture.GetComponent<Renderer>();

                /*
                Logger.PrintMessage(LogLevel.Debug, "vs={0} scale={1} tx scale={2}",
                    viewScale, viewScale, renderer.material.mainTextureScale.x);
                */

                transform.localScale = RangeUnitScale * (float)viewScale;

                m_lastRange = Range;
                m_lastZoom = MapController.Instance.MapView.TileDriver.Zoom;
            }
        }

        // Update is called once per frame
        void Update()
        {
            SetScale();
        }
    }

}