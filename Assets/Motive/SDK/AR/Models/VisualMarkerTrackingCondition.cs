// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Models
{
    public class VisualMarkerTrackingCondition : AtomicCondition, IMediaItemProvider
    {
        public IVisualMarker[] VisualMarkers { get; set; }

        public void GetMediaItems(IList<Core.Media.MediaItem> items)
        {
            if (VisualMarkers != null)
            {
                foreach (var marker in VisualMarkers)
                {
                    var mp = marker as IMediaItemProvider;

                    if (mp != null)
                    {
                        mp.GetMediaItems(items);
                    }
                }
            }
        }
    }
}
