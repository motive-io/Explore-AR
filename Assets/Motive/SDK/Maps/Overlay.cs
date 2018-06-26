// Copyright (c) 2018 RocketChicken Interactive Inc.

using Motive.AR.LocationServices;
using Motive.AR.Models;

namespace Motive.Unity.Maps
{
    public class Overlay : IOverlay
    {
        public Coordinates Coordinates
        {
            get
            {
                if (MapOverlay != null && this.MapOverlay.CenterLocation != null)
                {
                    return MapOverlay.CenterLocation.Coordinates;
                }

                return null;
            }
        }

        public MapOverlay MapOverlay { get; private set; }

        public MapArea Area { get; private set; }

        public Overlay(MapOverlay overlay)
        {
            this.MapOverlay = overlay;
            this.Area = new MapArea(overlay.Size.X, overlay.Size.Y);
        }
    }
}
