// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Scripting;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Add or remove items from a player's inventory.
    /// </summary>
    public class InventoryCollectibles : ScriptObject
    {
        public CollectibleCount[] CollectibleCounts { get; set; }
        /// <summary>
        /// If set, simply set all of the inventory values to the specified count.
        /// Otherwise, treat the count as a delta.
        /// </summary>
        public bool StartAtZero { get; set; }
    }
}