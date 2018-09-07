
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Motive.Core.Scripting;

namespace Motive.Core.Models
{
    public class NullableVector : ScriptObject
    {
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Z { get; set; }
    }
}


