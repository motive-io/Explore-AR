// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Globalization;
using Motive.Core.Media;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Base class for quiz responses.
    /// </summary>
    public class QuizResponseBase : IMediaItemProvider
    {
        public LocalizedMedia LocalizedImage { get; set; }
        public LocalizedText ResponseText { get; set; }

        /// <summary>
        /// Optional events fired when this response is selected.
        /// </summary>
        public string[] Event { get; set; }

        public string Text
        {
            get { return LocalizedText.GetText(ResponseText); }
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
            if (LocalizedImage != null)
            {
                LocalizedImage.GetMediaItems(items);
            }
        }
    }
}
