// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Globalization;
using Motive.Core.Media;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Models
{
    public class MapLocationCollectionMechanic : ScriptObject, ILocationCollectionMechanic, IMediaItemProvider
    {
        public LocalizedMedia LocalizedSound { get; set; }

        public MediaItem Sound
        {
            get
            {
                return LocalizedMedia.GetMediaItem(LocalizedSound);
            }
        }

        public string SoundUrl
        {
            get
            {
                var sound = Sound;

                return sound != null ? sound.Url : null;
            }
        }

        public ScriptObject CollectAction { get; set; }

        public void GetMediaItems(IList<MediaItem> items)
        {
            LocalizedMedia.GetMediaItems(LocalizedSound, items);
        }
    }
}
