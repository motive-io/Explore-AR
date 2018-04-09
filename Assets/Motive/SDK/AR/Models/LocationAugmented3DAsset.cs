// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Models
{
    public class LocationAugmented3DAsset : Location3DAsset, ILocationAugmentedOptions
    {
        public double Elevation { get; set; }

        public bool AlwaysFaceViewer { get; set; }

        public IAugmentedDistanceVariation DistanceVariation { get; set; }

        public DoubleRange VisibleRange { get; set; }
    }
}
