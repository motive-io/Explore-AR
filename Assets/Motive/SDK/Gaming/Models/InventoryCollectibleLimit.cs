// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Used to script a limit on a particular inventory item.
    /// </summary>
    public class InventoryCollectibleLimit : ScriptObject
    {
        public NumericalOperator Operator { get; set; }
        public CollectibleCount[] CollectibleCounts { get; set; }
    }
}
