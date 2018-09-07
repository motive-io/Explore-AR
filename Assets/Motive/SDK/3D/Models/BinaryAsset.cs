// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections.Generic;
using Motive.Core.Globalization;
using Motive.Core.Media;
using Motive.Core.Scripting;

namespace Motive._3D.Models
{
    public class BinaryAsset : BaseAsset, IMediaItemProvider
    {
        public LocalizedMedia LocalizedMedia { get; set; }

        public string Url
        {
            get
            {
                return LocalizedMedia.GetMediaUrl(LocalizedMedia);
            }
        }

        public void GetMediaItems(IList<MediaItem> items)
        {
            LocalizedMedia.GetMediaItems(LocalizedMedia, items);
        }
    }
}
