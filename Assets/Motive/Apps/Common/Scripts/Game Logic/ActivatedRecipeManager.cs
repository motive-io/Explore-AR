// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core;
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.Core.Json;
using Motive.Core.Storage;
using Motive.Unity.Scripting;
using Motive.Unity.Storage;

namespace Motive.Unity.Gaming
{
    class ActivatedRecipeState
    {
        public Recipe Recipe { get; set; }
    }

    class ActivatedRecipeStateList
    {
        public ActivatedRecipeState[] Items { get; set; }
    }

    public class ActivatedRecipeManager : Singleton<ActivatedRecipeManager>
    {
        private IStorageAgent m_fileAgent;
        public EventHandler Updated;

        /// <summary>
        /// Handle on activated recipes.
        /// </summary>
        protected SetDictionary<string, Recipe> m_recipesByCollectible;
        protected Dictionary<string, Recipe> m_recipesById;

        public ActivatedRecipeManager()
        {
            m_recipesByCollectible = new SetDictionary<string, Recipe>();
            m_recipesById = new Dictionary<string, Recipe>();

            m_fileAgent = StorageManager.GetGameStorageAgent("ActivatedRecipeManager.json");

            var state = JsonHelper.Deserialize<ActivatedRecipeStateList>(m_fileAgent);

            if (state != null && state.Items != null)
            {
                foreach (var recInfo in state.Items)
                {
                    if (recInfo.Recipe == null)
                    {
                        continue;
                    }

                    AddRecipe(recInfo.Recipe);

                    // Check to see if this recipe is in the directory. If it is, use the
                    // item in the directory. Otherwise use the item we saved.
                    var recipe = RecipeDirectory.Instance.GetItem(recInfo.Recipe.Id) ?? recInfo.Recipe;

                    AddRecipe(recipe);
                }

                Save();
            }

            ScriptManager.Instance.ScriptsReset += Instance_ScriptsReset;
        }

        /// <summary>
        /// Returns Activated Recipes.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Recipe> GetRecipes()
        {
            lock (m_recipesById)
            {
                return m_recipesById.Values;
            }
        }

        public Recipe GetRecipe(string id)
        {
            lock (m_recipesById)
            {
                if (m_recipesById.ContainsKey(id))
                {
                    return m_recipesById[id];
                }
            }

            return null;
        }

        public bool HasRecipe(string collectibleId)
        {
            return m_recipesByCollectible.ContainsKey(collectibleId);
        }

        public IEnumerable<Recipe> GetRecipesForCollectible(string collectibleId)
        {
            return m_recipesByCollectible[collectibleId];
        }

        public bool IsRecipeActivated(string recipeId)
        {
            return m_recipesById.ContainsKey(recipeId);
        }

        void AddRecipe(Recipe recipe)
        {
            lock (m_recipesById)
            {
                m_recipesById[recipe.Id] = recipe;

                if (recipe.Output == null || recipe.Output.CollectibleCounts == null) return;

                // each recipe can have multiple outputs, register each
                foreach (var cc in recipe.Output.CollectibleCounts)
                {
                    m_recipesByCollectible.Add(cc.CollectibleId, recipe);
                }
            }
        }

        public void ActivateRecipe(Recipe recipe)
        {
            AddRecipe(recipe);

            Save();

            if (Updated != null)
            {
                Updated(this, EventArgs.Empty);
            }
        }

        void RemoveRecipe(string recipeId)
        {
            lock (m_recipesById)
            {
                Recipe recipe;

                if (m_recipesById.TryGetValue(recipeId, out recipe))
                {
                    m_recipesById.Remove(recipe.Id);

                    if (recipe.Output == null || recipe.Output.CollectibleCounts == null) return;

                    // each recipe can have multiple outputs, remove each
                    foreach (var cc in recipe.Output.CollectibleCounts)
                    {
                        m_recipesByCollectible.Remove(cc.CollectibleId, recipe);
                    }
                }
            }
        }

        public void DeactivateRecipe(string recipeId)
        {
            RemoveRecipe(recipeId);

            Save();

            if (Updated != null)
            {
                Updated(this, EventArgs.Empty);
            }
        }

        void Instance_ScriptsReset(object sender, EventArgs e)
        {
            lock (m_recipesById)
            {
                m_recipesById.Clear();
                m_recipesByCollectible.Clear();
            }

            //Save();

            if (Updated != null)
            {
                Updated(this, e);
            }
        }

        public void Save()
        {
            ActivatedRecipeState[] recipes;

            lock (m_recipesById)
            {
                recipes = m_recipesById.Values.Select(
                    r =>
                    new ActivatedRecipeState
                    {
                        Recipe = r
                    })
                    .ToArray();
            }

            JsonHelper.Serialize(m_fileAgent, new ActivatedRecipeStateList()
            {
                Items = recipes
            });
        }
    }
}