// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Media;
using Motive.Core.Models;
using Motive.Core.Scripting;
using System.Collections.Generic;
using System.Linq;

namespace Motive._3D.Models
{
    /// <summary>
    /// Applies an effect (like an animation) to a World Object.
    /// </summary>
    public class WorldObjectEffectPlayer : ScriptObject, IMediaItemProvider
    {
        public ObjectReference[] WorldObjectReferences { get; set; }
        public ScriptObject[] Effects{ get; set; }

        public void GetMediaItems(IList<MediaItem> items)
        {
            if (Effects != null)
            {
                foreach (var provider in Effects.OfType<IMediaItemProvider>())
                {
                    provider.GetMediaItems(items);
                }
            }
        }
    }
}
