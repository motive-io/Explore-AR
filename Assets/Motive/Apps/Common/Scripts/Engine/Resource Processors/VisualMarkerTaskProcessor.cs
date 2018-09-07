// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Core.Scripting;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class VisualMarkerTaskProcessor : ScriptResourceProcessor<VisualMarkerTask>
    {
        public override void ActivateResource(ResourceActivationContext context, VisualMarkerTask resource)
        {
            TaskManager.Instance.ActivateVisualMarkerTask(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, VisualMarkerTask resource)
        {
            TaskManager.Instance.DeactivateTask(context.InstanceId);
        }

        public override void UpdateResource(ResourceActivationContext context, VisualMarkerTask resource)
        {
            TaskManager.Instance.UpdateTask(context, resource);
        }
    }
}