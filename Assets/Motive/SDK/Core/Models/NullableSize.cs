using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Motive.Core.Scripting;

namespace Motive.Core.Models
{
    public class NullableSize : ScriptObject
    {

        public double? Height { get; set; }
        public double? Width { get; set; }
        public double? Depth { get; set; }
    }

}
