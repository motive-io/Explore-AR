// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// A condition that is met when the player has the required
    /// items in their inventory to craft the specified collectibles.
    /// </summary>
    public class CanCraftCollectibleCondition : AtomicCondition
    {
        public CollectibleCount CollectibleCount { get; set; }
    }
}

