// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Scripting;
using Motive.Core.Models;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Condition that is met when the specified recipe has been activated.
    /// </summary>
	public class RecipeActivatedCondition : AtomicCondition
	{
		public ObjectReference RecipeReference { get; set; }
	}
}

