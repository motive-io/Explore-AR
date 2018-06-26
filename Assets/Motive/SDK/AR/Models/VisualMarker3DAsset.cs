// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.Core.Media;
using Motive.Core.Models;
using Motive._3D.Models;
using Motive.UI;

namespace Motive.AR.Models
{
    public class VisualMarker3DAsset : ScriptObject, IMediaItemProvider
    {
        public IVisualMarker[] Markers { get; set; }
        public AssetInstance AssetInstance { get; set; }
        public MediaElement FallbackImage { get; set; }

        public void GetMediaItems(IList<MediaItem> items)
        {
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

            if (AssetInstance != null)
            {
                AssetInstance.GetMediaItems(items);
            }
        }
    }
}
