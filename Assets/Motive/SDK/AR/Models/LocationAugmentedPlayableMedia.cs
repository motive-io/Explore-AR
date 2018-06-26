// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Models
{
    public class LocationAugmentedPlayableMedia : LocationAugmentedImage
    {
        public bool Loop { get; set; }
        public bool PlayWhenViewable { get; set; }
    }
}
