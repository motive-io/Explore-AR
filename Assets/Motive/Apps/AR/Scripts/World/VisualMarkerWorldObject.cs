// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.AR
{
    public class VisualMarkerWorldObject : ARWorldObject
    {
        public IVisualMarker Marker { get; set; }
    }
}
