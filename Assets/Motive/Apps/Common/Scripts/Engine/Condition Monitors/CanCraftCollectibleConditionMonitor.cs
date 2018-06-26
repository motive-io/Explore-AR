// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Scripting;
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using System.Collections.Generic;
using System.Linq;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class CanCraftCollectibleConditionMonitor : SynchronousConditionMonitor<CanCraftCollectibleCondition>
    {
        public CanCraftCollectibleConditionMonitor()
            : base("motive.gaming.canCraftCollectibleCondition")
        {
            Inventory.Instance.Updated += UpdatedEventHandler;
            Wallet.Instance.Updated += UpdatedEventHandler;
        }

        void UpdatedEventHandler(object sender, System.EventArgs args)
        {
            CheckWaitingConditions();
        }

        public override bool CheckState(FrameOperationContext fop, CanCraftCollectibleCondition condition, out object[] results)
        {
            if (condition.CollectibleCount != null)
            {
                var recipes = ActivatedRecipeManager.Instance.GetRecipesForCollectible(condition.CollectibleCount.CollectibleId);

                if (recipes != null)
                {
                    var canCraft = recipes.Where(
                        r => CraftingHelper.GetMaxNumberOfCrafts(r) >= condition.CollectibleCount.Count)
                        .ToArray();

                    if (canCraft.Length >= 1)
                    {
                        results = canCraft;
                        return true;
                    }
                }
            }

            results = null;

            return false;
        }
    }    
}