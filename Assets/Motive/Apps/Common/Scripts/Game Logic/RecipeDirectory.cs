// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core;
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using System.Collections.Generic;
using System.Linq;

namespace Motive.Unity.Gaming
{
    public class RecipeDirectory : SingletonAssetDirectory<RecipeDirectory, Recipe>
    {
        protected SetDictionary<string, Recipe> m_recipesByCollectible;

        public RecipeDirectory()
        {
            m_recipesByCollectible = new SetDictionary<string, Recipe>();
        }

        protected override void AddItem(Recipe item)
        {
            if (item.Output != null && item.Output.CollectibleCounts != null)
            {
                foreach (var cc in item.Output.CollectibleCounts)
                {
                    m_recipesByCollectible.Add(cc.CollectibleId, item);
                }
            }

            base.AddItem(item);
        }

        public bool HasRecipes(string collectibleId)
        {
            return m_recipesByCollectible.ContainsKey(collectibleId);
        }

        public IEnumerable<Recipe> GetRecipesForCollectible(string collectibleId)
        {
            return m_recipesByCollectible[collectibleId];
        }

        public Recipe GetRecipeForCollectible(string collectibleId)
        {
            var recipes = m_recipesByCollectible[collectibleId];

            if (recipes != null)
            {
                return recipes.FirstOrDefault();
            }

            return null;
        }
    }

}