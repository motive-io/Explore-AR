// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class PlayerTaskProcessor : ThreadSafeScriptResourceProcessor<PlayerTask>
    {
        public override void ActivateResource(ResourceActivationContext context, PlayerTask resource)
        {
            TaskManager.Instance.ActivatePlayerTask(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, PlayerTask resource)
        {
            TaskManager.Instance.DeactivateTask(context.InstanceId);
        }
    }
}