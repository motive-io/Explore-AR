// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Media;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Vuforia
{
    public class MarkerDatabase : ScriptObject, IMediaItemProvider
    {
        public MediaItem DataFile { get; set; }
        public MediaItem XmlFile { get; set; }

        public void GetMediaItems(IList<MediaItem> items)
        {
            if (DataFile != null)
            {
                items.Add(DataFile);
            }

            if (XmlFile != null)
            {
                items.Add(XmlFile);
            }
        }
    }
}
