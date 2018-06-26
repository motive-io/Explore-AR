// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Vuforia
{
    public class VuMarkTarget : ScriptObject, IMediaItemProvider
    {
        public MarkerDatabase Database { get; set; }
        public string Name { get; set; }

        public void GetMediaItems(IList<Core.Media.MediaItem> items)
        {
            if (Database != null)
            {
                Database.GetMediaItems(items);
            }
        }
    }
}
