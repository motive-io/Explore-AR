// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Models
{
    public interface ILocationAugmentedOptions
    {
        IAugmentedDistanceVariation DistanceVariation { get; }
        bool AlwaysFaceViewer { get; }
        DoubleRange VisibleRange { get; }
    }
}
