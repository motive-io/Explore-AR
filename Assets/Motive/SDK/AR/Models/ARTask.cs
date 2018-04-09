// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.Core.Models;
using Motive.Gaming.Models;

namespace Motive.AR.Models
{
    public class ARTask : CharacterTask
    {
        public ObjectReference[] ARObjectReferences { get; set; }

        public DoubleRange ActionRange { get; set; }
    }
}
