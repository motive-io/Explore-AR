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
    public class LocationAttractionInteractible : LocationAttractionItemProperties
    {
        public IScriptObject InteractibleItem { get; set; }

        public override void GetMediaItems(IList<Core.Media.MediaItem> items)
        {
            base.GetMediaItems(items);

            var provider = InteractibleItem as IMediaItemProvider;

            if (provider != null)
            {
                provider.GetMediaItems(items);
            }
        }

        public override IScriptObject GetItem()
        {
            return InteractibleItem;
        }
    }
}
