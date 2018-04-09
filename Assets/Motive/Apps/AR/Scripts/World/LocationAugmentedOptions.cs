// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.AR
{
    /// <summary>
    /// Concrete implementation of ILocationAugmentedOptions
    /// </summary>
    public class LocationAugmentedOptions : ILocationAugmentedOptions
    {
        /// <summary>
        /// If true, this object should always face the viewer.
        /// </summary>
        public bool AlwaysFaceViewer { get; set; }

        /// <summary>
        /// Describes how to vary the distance of the object from the viewer in the AR mode.
        /// </summary>
        public IAugmentedDistanceVariation DistanceVariation { get; set; }

        /// <summary>
        /// Only show AR objects if they're in this range.
        /// </summary>
        public DoubleRange VisibleRange { get; set; }

        public static LocationAugmentedOptions GetFixedDistanceOptions(double distance, bool alwaysFaceViewer = false, DoubleRange visibleRange = null)
        {
            return new LocationAugmentedOptions
            {
                AlwaysFaceViewer = alwaysFaceViewer,
                DistanceVariation = new FixedDistanceVariation { Distance = distance },
                VisibleRange = visibleRange
            };
        }

        public static LocationAugmentedOptions GetLinearDistanceOptions(bool alwaysFaceViewer, DoubleRange visibleRange = null)
        {
            return GetLinearDistanceOptions(null, null, null, alwaysFaceViewer, visibleRange);
        }

        public static LocationAugmentedOptions GetLinearDistanceOptions(double? scale = null, double? min = null, double? max = null, bool alwaysFaceViewer = false, DoubleRange visibleRange = null)
        {
            return new LocationAugmentedOptions
            {
                AlwaysFaceViewer = alwaysFaceViewer,
                DistanceVariation = new LinearDistanceVariation
                {
                    Range = new DoubleRange
                    {
                        Min = min,
                        Max = max
                    },
                    Scale = scale
                },
                VisibleRange = visibleRange
            };
        }
    }
}