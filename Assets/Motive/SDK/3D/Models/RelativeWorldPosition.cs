// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Scripting;

namespace Motive._3D.Models
{
    /// <summary>
    /// Defines a position relative to the player in a 3D or AR scene.
    /// </summary>
    public class RelativeWorldPosition : ScriptObject
    {
        public ObjectReference AnchorObjectReference { get; set; }
        public double Angle { get; set; }
        public double Distance { get; set; }
        public double Elevation { get; set; }
    }
}
