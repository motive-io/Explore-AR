// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Scripting;
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    /// <summary>
    /// Monitors the player's inventory and determines whether or not 
    /// inventory conditions are met.
    /// </summary>
    public class InventoryConditionMonitor : ConditionMonitor<InventoryCondition>
    {
        public InventoryConditionMonitor() : base("motive.gaming.inventoryCondition")
        {
            Inventory.Instance.Updated += Instance_Updated;
        }

        private void Instance_Updated(object sender, System.EventArgs e)
        {
            // The base ConditionMonitor class keeps track of all waiting conditions. Calling
            // CheckWaitingConditions re-processes each one, calling CheckState below.
            CheckWaitingConditions();
        }

        public override void CheckState(FrameOperationContext fop, InventoryCondition condition, ConditionCheckStateComplete onComplete)
        {
            // Get the current count of the item in the inventory
            var count = Inventory.Instance.GetCount(condition.CollectibleCount.CollectibleId);

            // Use MathHelper to compare this count with the count in the condition, given the
            // numerical condition operator
            var result = MathHelper.CheckNumericalCondition(condition.Operator, count, condition.CollectibleCount.Count);

            // Report back whether or not this condition is met
            onComplete(result, null);
        }
    }

}