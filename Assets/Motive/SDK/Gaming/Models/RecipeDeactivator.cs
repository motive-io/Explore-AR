// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Scripting;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Deactivates a recipe.
    /// </summary>
	public class RecipeDeactivator : ScriptObject
	{
		public ObjectReference[] RecipeReferences	 { get; set; }
	}
}

