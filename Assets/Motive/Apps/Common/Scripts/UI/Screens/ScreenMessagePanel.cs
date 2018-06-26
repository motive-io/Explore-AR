// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Media;
using UnityEngine.UI;
using Motive.Unity.Media;
using System;
using Motive.Core.Models;
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.Unity.Utilities;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays Screen Messages.
    /// </summary>
    public class ScreenMessagePanel : Panel<ResourcePanelData<ScreenMessage>>
    {
        public override void Populate(ResourcePanelData<ScreenMessage> data)
        {
            PopulateComponent<ScreenMessageComponent>(data.Resource);
        }

        public void Select()
        {
            Data.ActivationContext.FireEvent("select");
        }
    }
}