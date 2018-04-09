// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Scripting;

namespace Motive.UI.Models
{
    /// <summary>
    /// Attach a persistent "inspector" to an object. An inspector is
    /// usually a canvas entity that displays some information.
    /// </summary>
    public class ObjectInspector : ScriptObject
    {
        public ObjectReference[] TargetReferences { get; set; }
        public IObjectInspectorContent Content { get; set; }
    }
}
