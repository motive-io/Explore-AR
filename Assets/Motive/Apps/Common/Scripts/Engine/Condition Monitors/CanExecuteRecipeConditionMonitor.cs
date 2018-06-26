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
    public class CanExecuteRecipeConditionMonitor :
SynchronousConditionMonitor<CanExecuteRecipeCondition>
    {
        public CanExecuteRecipeConditionMonitor()
            : base("motive.gaming.canExecuteRecipeCondition")
        {
            ActivatedRecipeManager.Instance.Updated += UpdatedEventHandler;
            Inventory.Instance.Updated += UpdatedEventHandler;
            Wallet.Instance.Updated += UpdatedEventHandler;
        }

        void UpdatedEventHandler(object sender, System.EventArgs args)
        {
            CheckWaitingConditions();
        }

        public override bool CheckState(FrameOperationContext fop, CanExecuteRecipeCondition condition, out object[] results)
        {
            results = null;

            if (condition.RecipeReference != null)
            {
                // First, is recipe active
                var recipe = ActivatedRecipeManager.Instance.GetRecipe(condition.RecipeReference.ObjectId);

                if (recipe != null)
                {
                    return CraftingHelper.GetMaxNumberOfCrafts(recipe) >=
                        condition.Count.GetValueOrDefault(1);
                }
            }

            return false;
        }
    }
}
