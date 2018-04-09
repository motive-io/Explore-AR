// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Globalization;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Motive.Gaming.Models
{
    /// <summary>
    /// An objective to be completed as part of an assignment.
    /// </summary>
    public class TaskObjective : ScriptObject, IMediaItemProvider
    {
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