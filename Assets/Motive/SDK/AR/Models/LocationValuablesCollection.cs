// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Models
{
    public class LocationValuablesCollection : ScriptObject, ILocationSpawnItemOptions
    {
        public Location[] Locations { get; set; }
        public ValuablesCollection ValuablesCollection { get; set; }
        public string[] LocationTypes { get; set; }
        public string[] StoryTags { get; set; }
        public float? Probability { get; set; }

        public LocationValuablesCollectionOptions CollectOptions { get; set; }

        public CollectibleCount[] GetCollectibleCounts()
        {
            if (ValuablesCollection.CollectibleCounts.Length > 0)
            {
                return ValuablesCollection.CollectibleCounts;
            }

            else
            {
                return null;
            }
        }

    }
}
