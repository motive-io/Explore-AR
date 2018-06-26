// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Media;
using Motive.UI.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// A base component for displaying data from a MediaItem object.
    /// </summary>
    public abstract class MediaItemComponent : PanelComponent<MediaItem>
    {
        public abstract override void Populate(MediaItem obj);
    }
}