// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Vuforia
{
    public interface IVuforiaMarker : IVisualMarker
    {
        MarkerDatabase Database { get; }
        VuMarkIdentifier Identifier { get; }
        bool UseExtendedTracking { get; }
    }
}
