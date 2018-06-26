// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.UI.Models;
using Motive.Unity.UI;

namespace Motive.Unity.Scripting
{
    public class InterfaceDirectorProcessor : ThreadSafeScriptResourceProcessor<InterfaceDirector>
    {
        public override void ActivateResource(ResourceActivationContext context, InterfaceDirector resource)
        {
            InterfaceDirectorManager.Instance.ProcessInterfaceDirector(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, InterfaceDirector resource)
        {
            base.DeactivateResource(context, resource);
        }
    }
}