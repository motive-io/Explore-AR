// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Vuforia
{
    public class FrameMarker : VisualMarker
    {
        public string Name { get; set; }
        public int MarkerId { get; set; }
    }
}
