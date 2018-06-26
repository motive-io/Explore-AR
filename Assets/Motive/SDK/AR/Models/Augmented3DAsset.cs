// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive._3D.Models;
using Motive.Core.Media;
using Motive.Core.Models;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Models
{
    public class Augmented3DAsset : ScriptObject, IMediaItemProvider, ILocationAugmentedOptions
    {
        public IScriptObject Position { get; set; }

        public AssetInstance AssetInstance { get; set; }

        public IAugmentedDistanceVariation DistanceVariation { get; set; }

        public bool AlwaysFaceViewer { get; set; }

        public DoubleRange VisibleRange { get; set; }

        public void GetMediaItems(IList<MediaItem> items)
        {
            if (AssetInstance != null)
            {
                AssetInstance.GetMediaItems(items);
            }
        }
    }
}
