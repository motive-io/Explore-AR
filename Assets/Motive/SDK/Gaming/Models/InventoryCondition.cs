// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Scripting;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// A condition that monitors the player's inventory.
    /// </summary>
    public class InventoryCondition : AtomicCondition
    {
        public NumericalConditionOperator Operator { get; set; }

        public CollectibleCount CollectibleCount { get; set; }
    }
}