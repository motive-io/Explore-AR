// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Media;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.Core.Models;

namespace Motive.AR.Models
{
    public class RawImageMarker : ScriptObject, IVisualMarker, IMediaItemProvider
    {
        public MediaItem TargetImage { get; set; }
        public string Name { get; set; }
        public Size2D ImageSize { get; set; }

        public string GetIdentifier()
        {
            return TargetImage == null ? "" : TargetImage.Id;
        }

        public void GetMediaItems(IList<MediaItem> items)
        {
            if (TargetImage != null)
            {
                items.Add(TargetImage);
            }
        }
    }
}
