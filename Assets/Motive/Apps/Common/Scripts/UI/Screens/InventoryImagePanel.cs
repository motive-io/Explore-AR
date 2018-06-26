// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Motive.UI.Framework;
using Motive.Core.Models;
using Motive.Gaming.Models;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Used for displaying inventory items with attached images.
    /// </summary>
    public class InventoryImagePanel : InventoryCollectiblePanel
    {
        protected override string GetImageUrl(Collectible data)
        {
            var collectible = data;

            if (collectible.Content is MediaContent)
            {
                var media = (MediaContent)collectible.Content;

                if (media.MediaItem != null &&
                    media.MediaItem.Url != null)
                {
                    return media.MediaItem.Url;
                }
            }

            return collectible.ImageUrl;
        }
    }

}