// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Scripting;
using Motive.Core.Models;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// A condition that is met if the specified collectible has an activated
    /// recipe that can be used to craft it.
    /// </summary>
    public class CollectibleHasRecipeCondition : AtomicCondition
    {
        public ObjectReference CollectibleReference { get; set; }
    }
}

