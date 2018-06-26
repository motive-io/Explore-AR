// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Gaming
{
    public class DefaultLocationMinigameDriver : LocationMinigameDriverBase
    {
        public DefaultLocationMinigameDriver(LocationTaskDriver driver) : base(driver) { }

        public override bool ShowActionButton
        {
            // By default, show action button for any action except "in range"
            get
            {
                return
                    TaskDriver.Task.Action != TaskAction.InRange &&
                    TaskDriver.Task.Action != TaskAction.Wait;
            }
        }

        public override bool ShowMapAnnotation
        {
            get { return !TaskDriver.Task.IsHidden; }
        }
    }

}