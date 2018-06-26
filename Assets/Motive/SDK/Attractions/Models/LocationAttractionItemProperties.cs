// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Globalization;
using Motive.Core.Models;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Attractions.Models
{
    public abstract class LocationAttractionItemProperties : ScriptObject, IMediaItemProvider
    {
        public ObjectReference<LocationAttraction>[] AttractionReferences { get; set; }

        public LocalizedText LocalizedTitle { get; set; }
        public LocalizedText LocalizedDescription { get; set; }
        public LocalizedMedia LocalizedImage { get; set; }

        public DoubleRange Range { get; set; }
        public AttractionRangeAvailability RangeAvailability { get; set; }
        public AttractionRangeAutoplayBehavior AutoplayBehavior { get; set; }

        public abstract IScriptObject GetItem();

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

        public virtual void GetMediaItems(IList<Core.Media.MediaItem> items)
        {
            LocalizedMedia.GetMediaItems(LocalizedImage, items);
        }
    }
}
