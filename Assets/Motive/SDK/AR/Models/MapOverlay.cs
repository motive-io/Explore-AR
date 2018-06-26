// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections.Generic;
using Motive.AR.LocationServices;
using Motive.Core.Media;
using Motive.Core.Models;
using Motive.Core.Scripting;
using Motive.UI;

namespace Motive.AR.Models
{
    public class MapOverlay : ScriptObject, IMediaItemProvider
    {
        public Vector Size { get; set; }

        public Location CenterLocation { get; set; }

        public MediaElement MediaElement { get; set; }

        public void GetMediaItems(IList<MediaItem> items)
        {
            if (MediaElement != null)
            {
                MediaElement.GetMediaItems(items);
            }
        }
    }
}
