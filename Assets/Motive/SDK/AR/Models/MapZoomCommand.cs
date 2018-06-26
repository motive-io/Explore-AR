// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.Core.Scripting;
using System;

namespace Motive.AR.Models
{
    public class MapZoomCommand : ScriptObject
    {
        public Location CenterLocation { get; set; }
        public Motive.Core.Models.Vector Span { get; set; }
        public TimeSpan? Duration { get; set; }
    }
}
