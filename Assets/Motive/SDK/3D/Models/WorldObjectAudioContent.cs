// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.Core.Media;

namespace Motive._3D.Models
{
    public class WorldObjectAudioContent : ScriptObject, IContent, IMediaItemProvider
    {
        public ObjectReference[] WorldObjectReferences { get; set; }
        public LocalizedAudioContent LocalizedAudioContent { get; set; }

        public void GetMediaItems(IList<MediaItem> items)
        {
            if (LocalizedAudioContent != null)
            {
                LocalizedAudioContent.GetMediaItems(items);
            }
        }
    }
}
