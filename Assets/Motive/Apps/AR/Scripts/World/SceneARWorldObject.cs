// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.AR
{
    /// <summary>
    /// An AR object placed relative to the user.
    /// </summary>
    public class SceneARWorldObject : ARWorldObject
    {
        public ARWorldObject AnchorObject { get; set; }
        public IScriptObject Position { get; set; }
    }
}
