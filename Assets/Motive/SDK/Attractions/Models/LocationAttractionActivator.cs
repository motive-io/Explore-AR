// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Attractions.Models
{
    public class LocationAttractionActivator : ScriptObject
    {
        public ObjectReference<LocationAttraction>[] AttractionReferences { get; set; }
        public DoubleRange Range { get; set; }
    }
}
