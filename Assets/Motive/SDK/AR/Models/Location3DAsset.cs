// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive._3D;
using Motive._3D.Models;
using Motive.AR.LocationServices;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.Core.Media;
using Motive.Core.Models;

namespace Motive.AR.Models
{
    public class Location3DAsset : ScriptObject, IMediaItemProvider
    {
        public Location[] Locations { get; set; }
        public AssetInstance AssetInstance { get; set; }

        public void GetMediaItems(IList<MediaItem> items)
        {
            if (AssetInstance != null && AssetInstance.Asset is IMediaItemProvider)
            {
                ((IMediaItemProvider)AssetInstance.Asset).GetMediaItems(items);
            }
        }
    }
}
