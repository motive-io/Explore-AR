// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Unity.Gaming;
using Motive.Unity.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Scripting
{
    public class InventoryCollectibleLimitProcessor : ThreadSafeScriptResourceProcessor<InventoryCollectibleLimit>
    {
        public override void ActivateResource(ResourceActivationContext context, InventoryCollectibleLimit resource)
        {
            if (context.IsFirstActivation)
            {
                Inventory.Instance.SetLimits(resource);
            }
        }
    }
}
