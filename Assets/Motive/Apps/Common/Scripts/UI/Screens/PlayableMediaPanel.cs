// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.UI.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Base panel for displaying playable media.
    /// </summary>
    public class PlayableMediaPanel : Panel<ResourcePanelData<MediaContent>>
    {
        public override void Populate(ResourcePanelData<MediaContent> data)
        {
            PopulateComponent<MediaContentComponent>(data.Resource);

            base.Populate(data);
        }
    }
}