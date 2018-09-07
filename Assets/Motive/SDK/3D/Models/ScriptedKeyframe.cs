// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using System;

namespace Motive._3D.Models
{
    public class ScriptedKeyframe : ScriptObject
    {
        public TimeSpan Duration { get; set; }

        public string Easing { get; set; }

        public IScriptObject[] Properties { get; set; }
    }
}
