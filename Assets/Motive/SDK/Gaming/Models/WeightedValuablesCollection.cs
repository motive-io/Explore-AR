// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Valuables collection with an optional weight. Used, for example, by
    /// LocationValuables to randomly place valuables at certain locations.
    /// </summary>
    public class WeightedValuablesCollection
    {
        public double Weight { get; set; }
        public ValuablesCollection ValuablesCollection { get; set; }

        public WeightedValuablesCollection()
        {
            Weight = 1;
        }
    }
}