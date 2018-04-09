// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Globalization;
using Motive.Core.Media;
using Motive.Core.Models;
using Motive.Core.Scripting;
using System.Collections.Generic;

namespace Motive.UI.Models
{
    /// <summary>
    /// A playable that shows a simple notification. When in the background, the
    /// notification will use the device's notification system. 
    /// </summary>
    public class Notification : ScriptObject, IContent, IMediaItemProvider
    {
        public LocalizedText LocalizedTitle { get; set; }
        public LocalizedText LocalizedMessage { get; set; }
        public LocalizedMedia LocalizedSound { get; set; }
        public LocalizedMedia LocalizedImage { get; set; }

        /// <summary>
        /// If true, also vibrate. A notification with no other content and vibrate=true
        /// will simply vibrate the device.
        /// </summary>
        public bool Vibrate { get; set; }

        public string Title
        {
            get
            {
                return LocalizedText.GetText(LocalizedTitle);
            }
        }

        public string Message
        {
            get
            {
                return LocalizedText.GetText(LocalizedMessage);
            }
        }

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

        public string ImageUrl
        {
            get
            {
                return LocalizedMedia.GetMediaUrl(LocalizedImage);
            }
        }

        public void GetMediaItems(IList<MediaItem> items)
        {
            LocalizedMedia.GetMediaItems(LocalizedSound, items);
        }
    }
}
