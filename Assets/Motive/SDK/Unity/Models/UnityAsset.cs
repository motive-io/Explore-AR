// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Media;
using Motive.Core.Scripting;
using System.Collections.Generic;

namespace Motive.Unity.Models
{
    /// <summary>
    /// An asset from an asset bundle. If no asset bundle is set,
    /// tries to load the asset by name from resources.
    /// </summary>
    public class UnityAsset : Motive._3D.Models.BaseAsset, IMediaItemProvider
    {
        public AssetBundle AssetBundle { get; set; }
        public string AssetName { get; set; }

        public void GetMediaItems(IList<MediaItem> items)
        {
            if (AssetBundle != null)
            {
                AssetBundle.GetMediaItems(items);
            }
        }
    }
}
