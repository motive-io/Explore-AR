// Copyright (c) 2018 RocketChicken Interactive Inc.
using JetBrains.Annotations;
using Motive.Gaming.Models;
using System.Linq;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Helper methods for crafting.
    /// </summary>
    static class CraftingHelper
    {
        public static int GetMaxNumberOfCrafts(Recipe recipe)
        {
            if (recipe == null)
            {
                return 0;
            }

            var min = int.MaxValue;
            var valuables = recipe.Ingredients;

            if (valuables.CollectibleCounts != null)
            {
                foreach (var cc in valuables.CollectibleCounts)
                {
                    var ct = Inventory.Instance.GetCount(cc.CollectibleId);

                    var numberOfCrafts = ct / cc.Count;

                    if (numberOfCrafts < min)
                    {
                        min = numberOfCrafts;
                    }
                }
            }

            if (valuables.CurrencyCounts != null)
            {
                foreach (var cc in valuables.CurrencyCounts)
                {
                    var ct = Wallet.Instance.GetCount(cc.Currency);

                    var numberOfCrafts = ct / cc.Count;

                    if (numberOfCrafts < min)
                    {
                        min = numberOfCrafts;
                    }
                }
            }

            return min == int.MaxValue ? 0 : min;
        }

        public static CraftingReason GetRecipeReason([CanBeNull] Recipe recipe)
        {

            if (recipe == null)
            {
                return CraftingReason.NoRecipe;
            }

            if (TransactionManager.Instance.HasValuables(recipe.Ingredients))
            {
                return CraftingReason.RequirementsMet;
            }

            //////////////////////////////////////////////////////////////////////
            // Everything under here is for requirements that have NOT been met./
            ////////////////////////////////////////////////////////////////////
            var requirements = recipe.Requirements;

            if (requirements == null)
            {
                return CraftingReason.RequirementsUnmet;
            }

            // Check currency reasons
            if (requirements.CurrencyCounts != null)
            {
                var cc = recipe.Requirements.CurrencyCounts
                    .FirstOrDefault(_cc => _cc.Currency == "xp");

                if (cc != null)
                {
                    return CraftingReason.XpUnmet;
                }

                // Todo:: Any other currency reasons?
            }

            return CraftingReason.RequirementsUnmet;
        }
    }
}