// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Models
{
    /// <summary>
    /// Vary distance linearly from the user.
    /// </summary>
    public class LinearDistanceVariation : ScriptObject, IAugmentedDistanceVariation
    {
        /// <summary>
        /// Defines the range of distances from the user in which this object will be placed.
        /// </summary>
        public DoubleRange Range { get; set; }
        /// <summary>
        /// Defines the distance scale. 1 => 1m = 1 unit of distance in Unity.
        /// </summary>
        public double? Scale { get; set; }
    }
}
