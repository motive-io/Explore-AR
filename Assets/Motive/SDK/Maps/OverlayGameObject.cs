// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI;
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Applies a layered overlay to the map.
    /// </summary>
    public class OverlayGameObject : MonoBehaviour
    {
        public Overlay Overlay { get; set; }
        public MapView MapView { get; set; }
        //public GameObject ImageLayer;
        public TiledImage Image;

        MapTileDriver m_tileDriver;
        MapView m_mapView;
        double m_lastZoom;
        bool m_isImageSet;

        void Awake()
        {
            m_mapView = MapController.Instance.MapView;
            m_tileDriver = m_mapView.TileDriver;
        }

        void SetImage()
        {
            // Find a matching layer to render
            if (Overlay.MapOverlay.MediaElement != null &&
                Image != null)
            {
                Image.LoadMedia(Overlay.MapOverlay.MediaElement);
            }
        }

        void SetSize()
        {
            if (m_tileDriver.Zoom != m_lastZoom)
            {
                m_lastZoom = m_tileDriver.Zoom;

                var hscale = m_tileDriver.GetDistanceScale(m_mapView.CenterCoordinates, Overlay.Area.Width);
                var vscale = m_tileDriver.GetDistanceScale(m_mapView.CenterCoordinates, Overlay.Area.Height);

                var xscale = (float)hscale * MapController.Instance.MapX.magnitude;
                var yscale = (float)vscale * MapController.Instance.MapY.magnitude;

                //Logger.PrintMessage(LogLevel.Debug, "hs={0} vs={1} xs={2} ys={3}",
                //    hscale, vscale, xscale, yscale);

                transform.localScale = new Vector2(xscale, yscale);
            }
        }

        void Update()
        {
            if (!m_isImageSet && Overlay != null)
            {
                SetImage();

                m_isImageSet = true;
            }

            SetSize();
        }
    }
}