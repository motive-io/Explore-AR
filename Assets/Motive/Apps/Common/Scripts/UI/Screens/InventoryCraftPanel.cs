// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using Motive.Unity.Gaming;
using Motive.Unity.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace Motive.Unity.UI
{
    /// <summary>
    /// A version of an InventoryPanel used for craftable inventory items.
    /// </summary>
    public class InventoryCraftPanel : InventoryPanel
    {
        public bool NeedActivatedRecipeToCraft = false;
        public UnityEvent OnSelectPlayerNeedsRecipe;
        public UnityEvent OnSelectNotCraftableItem;

        protected override void Awake()
        {
            if (OnSelectNotCraftableItem == null)
            {
                OnSelectNotCraftableItem = new UnityEvent();
            }
            if (OnSelectPlayerNeedsRecipe == null)
            {
                OnSelectPlayerNeedsRecipe = new UnityEvent();
            }

            base.Awake();
        }

        public override IEnumerable<InventoryCollectible> FilterItems(IEnumerable<InventoryCollectible> input)
        {
            if (NeedActivatedRecipeToCraft)
            {
                return input.Where(i => ActivatedRecipeManager.Instance.HasRecipe(i.Collectible.Id));
            }

            return base.FilterItems(input);
        }

        protected override void SelectItem(InventoryTableItem item)
        {
            IEnumerable<Recipe> recipes;

            if (NeedActivatedRecipeToCraft)
            {
                recipes = ActivatedRecipeManager.Instance.GetRecipesForCollectible(item.InventoryCollectible.Collectible.Id);
            }
            else
            {
                recipes = RecipeDirectory.Instance.GetRecipesForCollectible(item.InventoryCollectible.Collectible.Id);
            }

            // For now only look at one recipe per collectible
            var recipe = (recipes == null) ? null : recipes.FirstOrDefault();

            if (recipe != null)
            {
                PopulateComponent<ExecuteRecipeComponent>(recipe);
            }
            else
            {
                var recipeExists = RecipeDirectory.Instance.HasRecipes(item.InventoryCollectible.Collectible.Id);
                if (recipeExists)
                {
                    if (OnSelectPlayerNeedsRecipe != null) OnSelectPlayerNeedsRecipe.Invoke();
                }
                else
                {
                    if (OnSelectNotCraftableItem != null) OnSelectNotCraftableItem.Invoke();
                }
            }
        }
    }

}