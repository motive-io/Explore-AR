// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.Core.Globalization;
using Motive.Core.Scripting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Motive.Attractions.Models
{
    public class LocationAttraction : ScriptObject, IMediaItemProvider
    {
        public Location[] Locations { get; set; }
        public LocalizedText LocalizedTitle { get; set; }
        public LocalizedText LocalizedDescription { get; set; }
        public LocalizedMedia LocalizedImage { get; set; }

        public string Title
        {
            get
            {
                return LocalizedText.GetText(LocalizedTitle);
            }
        }

        public string Description
        {
            get
            {
                return LocalizedText.GetText(LocalizedDescription);
            }
        }

        public string ImageUrl
        {
            get
            {
                return LocalizedMedia.GetMediaUrl(LocalizedImage);
            }
        }

        public void GetMediaItems(IList<Core.Media.MediaItem> items)
        {
            LocalizedMedia.GetMediaItems(LocalizedImage, items);
        }
    }
}
