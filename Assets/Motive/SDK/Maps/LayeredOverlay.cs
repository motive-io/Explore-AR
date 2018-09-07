// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.AR.Models;

namespace Motive.Unity.Maps
{
    public class LayeredOverlay : IOverlay
    {
        public Coordinates Coordinates
        {
            get
            {
                if (Overlay != null && Overlay.CenterLocation != null)
                {
                    return Overlay.CenterLocation.Coordinates;
                }

                return null;
            }
        }

        public LayeredMapOverlay Overlay { get; private set; }

        public MapArea Area { get; private set; }

        public LayeredOverlay(LayeredMapOverlay overlay)
        {
            this.Overlay = overlay;
            this.Area = new MapArea(overlay.Size.X, overlay.Size.Y);
        }
    }
}
