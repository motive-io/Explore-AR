// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Globalization;
using Motive.Core.Media;
using Motive.Core.Models;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Base class for responses that take text, media, and an image.
    /// </summary>
    public class TextMediaResponse : IMediaItemProvider
    {
        public string Event { get; set; }
        public LocalizedText LocalizedText { get; set; }
        public LocalizedMedia LocalizedMedia { get; set; }
        public LocalizedMedia LocalizedImage { get; set; }

        public string Text
        {
            get { return LocalizedText.GetText(LocalizedText); }
        }
        public MediaItem MediaItem
        {
            get { return LocalizedMedia.GetMediaItem(LocalizedMedia); }
        }
        public string ImageUrl
        {
            get
            {
                return LocalizedMedia.GetMediaUrl(LocalizedImage);
            }
        }

        public void GetMediaItems(IList<MediaItem> items)
        {
            if (LocalizedMedia != null)
            {
                LocalizedMedia.GetMediaItems(items);
            }

            if (LocalizedImage != null)
            {
                LocalizedImage.GetMediaItems(items);
            }
        }
    }
}