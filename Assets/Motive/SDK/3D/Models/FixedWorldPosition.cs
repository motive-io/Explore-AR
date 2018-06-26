// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Scripting;

namespace Motive._3D.Models
{
    /// <summary>
    /// Defines a fixed position in a 3D scene.
    /// </summary>
    public class FixedWorldPosition : ScriptObject
    {
        public Vector Position { get; set; }
    }
}
