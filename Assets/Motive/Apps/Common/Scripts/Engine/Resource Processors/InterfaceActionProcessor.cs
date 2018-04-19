// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.UI.Models;
using Motive.Unity.Scripting;
using Motive.Unity.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.Gaming.Models;

namespace Motive.Unity.Scripting
{
    public class InterfaceActionProcessor : ThreadSafeScriptResourceProcessor<InterfaceAction>
    {
        public override void ActivateResource(ResourceActivationContext context, InterfaceAction resource)
        {
            base.ActivateResource(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, InterfaceAction resource)
        {
            base.DeactivateResource(context, resource);
        }
    }
}