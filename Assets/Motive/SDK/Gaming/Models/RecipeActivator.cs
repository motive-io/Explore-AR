// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Activates a recipe.
    /// </summary>
    public class RecipeActivator : ScriptObject
    {
        public Recipe[] Recipes { get; set; }
    }
}
