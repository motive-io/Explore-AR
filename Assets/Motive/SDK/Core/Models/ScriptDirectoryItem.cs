// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Globalization;
using Motive.Core.Media;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;

namespace Motive.Core.Models
{
    /// <summary>
    /// Usually used to represent a quest or a tour, but can be configured to
    /// represent purchasable scripted content.
    /// </summary>
    public class ScriptDirectoryItem : ScriptObject, IMediaItemProvider
    {
        public ObjectReference ScriptReference { get; set; }

        public LocalizedText LocalizedTitle { get; set; }
        
        public int Order { get; set; }

        public TimeSpan? EstimatedDuration { get; set; }

        public PublishingStatus PublishingStatus { get; set; }

        /// <summary>
        /// Used to link this script directory item to an IAP purchase identifier.
        /// </summary>
        public string ProductIdentifier { get; set; }

        /// <summary>
        /// If set, the script reference will launch when the app starts.
        /// </summary>
        public bool Autoplay { get; set; }

        public string Title
        {
            get
            {
                return LocalizedText.GetText(LocalizedTitle);
            }
        }

        public LocalizedText LocalizedDescription { get; set; }

        public string Description
        {
            get
            {
                return LocalizedText.GetText(LocalizedDescription);
            }
        }

        public LocalizedMedia LocalizedBackgroundImage { get; set; }

        public string BackgroundImageUrl
        {
            get
            {
                return LocalizedMedia.GetMediaUrl(LocalizedBackgroundImage);
            }
        }

        public void GetMediaItems(IList<MediaItem> items)
        {
            LocalizedMedia.GetMediaItems(LocalizedBackgroundImage, items);
        }
    }
}
