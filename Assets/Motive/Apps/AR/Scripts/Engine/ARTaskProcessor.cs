// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Core.Scripting;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class ARTaskProcessor : ScriptResourceProcessor<ARTask>
    {
        public override void ActivateResource(Motive.Core.Scripting.ResourceActivationContext context, ARTask resource)
        {
            TaskManager.Instance.ActivateARTask(context, resource);
        }

        public override void DeactivateResource(Motive.Core.Scripting.ResourceActivationContext context, ARTask resource)
        {
            TaskManager.Instance.DeactivateTask(context.InstanceId);
        }

        public override void UpdateResource(Motive.Core.Scripting.ResourceActivationContext context, ARTask resource)
        {
            TaskManager.Instance.UpdateTask(context, resource);
        }
    }
}