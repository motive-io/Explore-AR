// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Unity.Scripting;
using Motive.Gaming.Models;
using Motive.Core.Scripting;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class InventoryCollectiblesProcessor : ThreadSafeScriptResourceProcessor<InventoryCollectibles>
    {
        public override void ActivateResource(ResourceActivationContext context, InventoryCollectibles resource)
        {
            if (!context.IsClosed)
            {
                // Add items to player's inventory
                if (resource.StartAtZero)
                {
                    Inventory.Instance.Set(resource.CollectibleCounts);
                }
                else
                {
                    Inventory.Instance.Add(resource.CollectibleCounts);
                }

                context.Close();
            }
        }
    }
}