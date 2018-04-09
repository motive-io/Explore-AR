// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Scripting;

namespace Motive._3D.Models
{
    /// <summary>
    /// A condition that monitors the user's relative position to a World Object.
    /// </summary>
    public class WorldObjectProximityCondition : AtomicCondition
    {
        public ObjectReference[] WorldObjectReferences { get; set; }
        public DoubleRange Range { get; set; }
    }
}
