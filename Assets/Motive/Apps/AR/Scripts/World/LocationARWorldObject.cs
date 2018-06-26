// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.AR.Models;
using Motive.Unity.Gaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.AR
{
    /// <summary>
    /// An AR object attached to a GPS point.
    /// </summary>
    public class LocationARWorldObject : ARWorldObject
    {
        /// <summary>
        /// Task driver for the AR object if it represents a Location Task.
        /// </summary>
        public LocationTaskDriver LocationTaskDriver { get; set; }

        /// <summary>
        /// The real-world location of this object.
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// Elevation of the object from the viewer.
        /// </summary>
        public double Elevation { get; set; }

        public Coordinates Coordinates
        {
            get
            {
                return Location != null ? Location.Coordinates : null;
            }
        }
    }
}
