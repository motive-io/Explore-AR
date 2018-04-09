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
    public class CollectibleHasRecipeConditionMonitor :
SynchronousConditionMonitor<CollectibleHasRecipeCondition>
    {
        public CollectibleHasRecipeConditionMonitor()
            : base("motive.gaming.collectibleHasRecipeCondition")
        {
            ActivatedRecipeManager.Instance.Updated += (sender, args) =>
            {
                CheckWaitingConditions();
            };
        }

        public override bool CheckState(FrameOperationContext fop, CollectibleHasRecipeCondition condition, out object[] results)
        {
            if (condition.CollectibleReference != null && condition.CollectibleReference.ObjectId != null)
            {
                var recipes = ActivatedRecipeManager.Instance.GetRecipesForCollectible(condition.CollectibleReference.ObjectId);

                if (recipes != null && recipes.Count() > 0)
                {
                    results = recipes.ToArray();

                    return true;
                }
            }

            results = null;

            return false;
        }
    }
}
