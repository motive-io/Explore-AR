// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Models
{
    public interface ILocationSpawnItemOptions //: IScriptObject
    {
        Location[] Locations { get; }
        string[] LocationTypes { get; }
        string[] StoryTags { get; }
        float? Probability { get; }
        //TimeSpan? ProbabilityResetTime { get; }
        //int? MaxItems { get; }
    }
}
