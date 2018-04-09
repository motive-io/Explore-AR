// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Gaming.Models;

namespace Motive.AR.Models
{
    public class LocationTreasureChest
    {
        public string[] LocationTypes { get; set; }
        public string[] StoryTags { get; set; }
        public double Weight { get; set; }
        public WeightedValuablesCollection[] TreasureChests { get; set; }
    }
}