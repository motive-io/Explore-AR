// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Models
{
    public class FixedDistanceVariation : ScriptObject, IAugmentedDistanceVariation
    {
        public double Distance { get; set; }
    }
}
