// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Attractions.Models
{
    public class LocationAttractionContent : LocationAttractionItemProperties
    {
        public IContent Content { get; set; }

        public override void GetMediaItems(IList<Core.Media.MediaItem> items)
        {
            base.GetMediaItems(items);

            if (Content != null && Content is IMediaItemProvider)
            {
                ((IMediaItemProvider)Content).GetMediaItems(items);
            }
        }

        public override IScriptObject GetItem()
        {
            return Content;
        }
    }
}
