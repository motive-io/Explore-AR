// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Scripting;
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class RecipeActivatedConditionMonitor : SynchronousConditionMonitor<RecipeActivatedCondition>
    {
        public RecipeActivatedConditionMonitor()
            : base("motive.gaming.recipeActivatedCondition")
        {
            ActivatedRecipeManager.Instance.Updated += (sender, args) =>
            {
                CheckWaitingConditions();
            };
        }

        public override bool CheckState(FrameOperationContext fop, RecipeActivatedCondition condition, out object[] results)
        {
            results = null;

            return ActivatedRecipeManager.Instance.IsRecipeActivated(condition.RecipeReference.ObjectId);
        }
    }

}
