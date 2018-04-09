// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.UI.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Base component for displaying data from a MediaContent object.
    /// </summary>
    public abstract class MediaContentComponent : PanelComponent<MediaContent>
    {
        public Text Title;

        public override void Populate(MediaContent obj)
        {
            if (Title)
            {
                Title.text = obj.Title;
            }

            base.Populate(obj);
        }
    }

}