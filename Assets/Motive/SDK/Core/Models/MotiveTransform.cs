// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;

namespace Motive.Core.Models
{
    public class MotiveTransform : ScriptObject
    {
        public NullableVector Scale { get; set; }
        public NullableVector Position { get; set; }
        public NullableVector Rotation { get; set; }
    }
}


