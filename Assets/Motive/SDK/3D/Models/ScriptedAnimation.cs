// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;

namespace Motive._3D.Models
{
    public class ScriptedAnimation : ScriptObject
    {
        public string Name { get; set; }

        public ScriptedKeyframe[] Keyframes { get; set; }

        public bool Loop { get; set; }
    }
}
