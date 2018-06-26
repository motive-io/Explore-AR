// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;

namespace Motive.Unity.Scripting
{
    public class InterfaceUpdateProcessor : ThreadSafeScriptResourceProcessor<InterfaceAction>
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