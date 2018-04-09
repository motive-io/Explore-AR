// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Unity.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays image MediaContent
    /// </summary>
    public class ImageContentComponent : MediaContentComponent
    {
        public RawImage Image;

        public override void Populate(MediaContent content)
        {
            if (content.MediaItem != null)
            {
                ImageLoader.LoadImageOnMainThread(content.MediaItem.Url, Image);
            }

            base.Populate(content);
        }
    }
}