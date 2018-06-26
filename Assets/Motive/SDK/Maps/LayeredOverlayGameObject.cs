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
    public class LayeredOverlayGameObject : MonoBehaviour
    {
        public LayeredOverlay Overlay { get; set; }
        public MapView MapView { get; set; }
        //public GameObject ImageLayer;
        public TiledImage Image;

        MapTileDriver m_tileDriver;
        MapView m_mapView;
        double m_lastZoom;
        Dictionary<string, Texture> m_imageCache;

        string m_lastUrl;

        void Awake()
        {
            m_mapView = MapController.Instance.MapView;
            m_tileDriver = m_mapView.TileDriver;
            m_imageCache = new Dictionary<string, Texture>();
        }

        void SetSize()
        {
            if (m_tileDriver.Zoom != m_lastZoom)
            {
                var hscale = m_tileDriver.GetDistanceScale(m_mapView.CenterCoordinates, Overlay.Area.Width);
                var vscale = m_tileDriver.GetDistanceScale(m_mapView.CenterCoordinates, Overlay.Area.Height);

                var xscale = (float)hscale * MapController.Instance.MapX.magnitude;
                var yscale = (float)vscale * MapController.Instance.MapY.magnitude;

                //Logger.PrintMessage(LogLevel.Debug, "hs={0} vs={1} xs={2} ys={3}",
                //    hscale, vscale, xscale, yscale);

                transform.localScale = new Vector2(xscale, yscale);

                //m_lastRange = Range;
                m_lastZoom = m_tileDriver.Zoom;
                
                MediaElement media = null;

                // Find a matching layer to render
                if (Overlay.Overlay.MapOverlayLayers != null)
                {
                    foreach (var layer in Overlay.Overlay.MapOverlayLayers)
                    {
                        if (layer.ZoomRange == null ||
                            (layer.ZoomRange != null && 
                            layer.ZoomRange.IsInRange(m_lastZoom)))
                        {
                            media = layer.MediaElement;

                            break;
                        }
                    }
                }

                if (Image &&
                    media != null &&
                    media.MediaUrl != null && 
                    media.MediaUrl != m_lastUrl)
                {
                    m_lastUrl = media.MediaUrl;

                    Image.LoadMedia(media);
                }
            }
        }

        void Update()
        {
            SetSize();
        }
    }
}