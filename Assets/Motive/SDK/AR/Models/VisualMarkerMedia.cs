// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Scripting;
using Motive.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.Core.Media;

namespace Motive.AR.Models
{
    public class VisualMarkerMedia : ScriptObject, IMediaItemProvider
    {
        public IVisualMarker[] Markers { get; set; }
        public MediaElement MediaElement { get; set; }

        public void GetMediaItems(IList<MediaItem> items)
        {
            MediaElement.GetMediaItems(MediaElement, items);

            if (Markers != null)
            {
                foreach (var marker in Markers)
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
