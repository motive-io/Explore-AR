// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.Core.Models;
using Motive.Core.Scripting;
using Motive.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Models
{
    public class LocationAugmentedImage : ScriptObject, ILocationAugmentedOptions, IMediaItemProvider
    {
        public MediaElement Marker { get; set; }

        public Location[] Locations { get; set; }

        public double Elevation { get; set; }

        public bool AlwaysFaceViewer { get; set; }

        public DoubleRange VisibleRange { get; set; }

        public IAugmentedDistanceVariation DistanceVariation { get; set; }

        public void GetMediaItems(IList<Core.Media.MediaItem> items)
        {
            if (Marker != null && Marker.LocalizedMedia != null)
            {
                Marker.LocalizedMedia.GetMediaItems(items);
            }
        }
    }
}
