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
    public class AugmentedMedia : ScriptObject, IMediaItemProvider, ILocationAugmentedOptions, IARMediaPlaybackProperties
    {
        public AugmentedMedia()
        {
            Volume = 1f;
        }

        public ScriptObject Position { get; set; }
        public MediaElement MediaElement { get; set; }

        public bool OnlyPlayWhenVisible { get; set; }
        public bool Loop { get; set; }
        public float Volume { get; set; }

        public IAugmentedDistanceVariation DistanceVariation { get; set; }

        public bool AlwaysFaceViewer { get; set; }

        public DoubleRange VisibleRange { get; set; }
        
        public void GetMediaItems(IList<MediaItem> items)
        {
            MediaElement.GetMediaItems(MediaElement, items);
        }
    }
}
