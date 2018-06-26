// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.Core.Scripting;
using Motive.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Models
{
    public class LocationMarker : ScriptObject, IMediaItemProvider
    {
        public MediaElement Marker { get; set; }

        public Location[] Locations { get; set; }

        public string[] Attributes { get; set; }

        public MarkerInformation Information { get; set; }

        public void GetMediaItems(IList<Motive.Core.Media.MediaItem> items)
        {
            if (Marker != null && Marker.LocalizedMedia != null)
            {
                Marker.LocalizedMedia.GetMediaItems(items);
            }

            if (Information != null)
            {
                Information.GetMediaItems(items);
            }
        }
    }
}
