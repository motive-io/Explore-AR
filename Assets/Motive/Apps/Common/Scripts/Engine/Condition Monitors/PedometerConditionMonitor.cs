// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Scripting;
using Motive.Core.Utilities;

namespace Motive.Unity.Scripting
{
    class PedometerConditionMonitor : ConditionMonitor<PedometerCondition>
    {
        public PedometerConditionMonitor() : base("motive.ar.pedometerCondition")
        {
            PedometerCalculator.Instance.Updated += Instance_Updated;
        }

        private void Instance_Updated(object sender, System.EventArgs e)
        {
            CheckWaitingConditions();
        }

        public override void CheckState(FrameOperationContext fop, PedometerCondition condition, ConditionCheckStateComplete onComplete)
        {
            bool res = false;
            float spm = PedometerCalculator.Instance.StepsPerMinute;

            if (condition.StepsPerMinuteRange != null)
            {
                res = condition.StepsPerMinuteRange.IsInRange(spm);
            }

            onComplete(res, null);
        }
    }
}
