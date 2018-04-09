// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Media;
using Motive.Core.Scripting;
using System.Collections.Generic;

namespace Motive.Unity.Models
{
    /// <summary>
    /// A Unity animation represented as a Motive UnityAsset.
    /// </summary>
    public class UnityAnimation : ScriptObject, IMediaItemProvider //, IWorldObjectEffect
    {
        public UnityAsset Asset { get; set; }

        public void GetMediaItems(IList<MediaItem> items)
        {
            if (Asset != null)
            {
                Asset.GetMediaItems(items);
            }
        }
    }
}
