// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.UI;

namespace Motive.AR.Models
{
    public class MapZoomCommand : ScriptObject
    {
        public Location CenterLocation { get; set; }
        public Motive.Core.Models.Vector Span { get; set; }
    }
}
