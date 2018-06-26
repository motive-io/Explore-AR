// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Globalization;
using Motive.Core.Media;
using Motive.Core.Models;
using Motive.Core.Scripting;
using Motive.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Motive.Gaming.Models
{
    /// <summary>
    /// Response to a screen message.
    /// </summary>
    public class ScreenMessageResponse : TextMediaResponse
    {

    }

    /// <summary>
    /// Represents a message pushed to the screen.
    /// </summary>
    public class ScreenMessage : TextMediaContent, IObjectInspectorContent
    {
        public LocalizedText LocalizedTitle { get; set; }
        public LocalizedText LocalizedSubtitle { get; set; }
        public ScreenMessageResponse[] Responses { get; set; }
        public LocalizedMedia LocalizedImage { get; set; }
        public LocalizedText LocalizedButtonText { get; set; }

        public string ImageUrl
        {
            get
            {
                return LocalizedMedia.GetMediaUrl(LocalizedImage);
            }
        }

        public string Title
        {
            get
            {
                return LocalizedText.GetText(LocalizedTitle);
            }
        }

        public string Subtitle
        {
            get
            {
                return LocalizedText.GetText(LocalizedSubtitle);
            }
        }

        public string ButtonText
        {
            get
            {
                return LocalizedText.GetText(LocalizedButtonText);
            }
        }

        public override void GetMediaItems(IList<MediaItem> items)
        {
            if (Responses != null)
            {
                foreach (var response in Responses)
                {
                    response.GetMediaItems(items);
                }
            }

            LocalizedMedia.GetMediaItems(LocalizedImage, items);

            base.GetMediaItems(items);
        }
    }
}