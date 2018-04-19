// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays inventory items with attached screen messages.
    /// </summary>
    public class InventoryScreenMessagePanel : InventoryCollectiblePanel
    {
        public override void PopulateContent(Collectible collectible)
        {
            PopulateComponent<ScreenMessageComponent>(collectible.Content);
        }
    }
}