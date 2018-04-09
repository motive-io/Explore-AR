// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Scripting;
using Motive.Core.Models;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// A condition that is met when a player can execute a recipe, 
    /// meaning they have the ingredients in their inventory and meet
    /// the input requirements.
    /// </summary>
    public class CanExecuteRecipeCondition : AtomicCondition
    {
        public ObjectReference RecipeReference { get; set; }
        public int? Count { get; set; }
    }
}
