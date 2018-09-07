// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;

namespace Motive.Core.Models
{
    class PedometerCondition: AtomicCondition
    {
        public DoubleRange StepsPerMinuteRange { get; set; }
    }
}
