// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.AR.Models;
using Motive.Core.Scripting;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class LocationTaskProcessor : ScriptResourceProcessor<LocationTask>
    {
        public override void ActivateResource(ResourceActivationContext context, LocationTask resource)
        {
            TaskManager.Instance.ActivateLocationTask(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, LocationTask resource)
        {
            TaskManager.Instance.DeactivateTask(context.InstanceId);
        }

        public override void UpdateResource(ResourceActivationContext context, LocationTask resource)
        {
            TaskManager.Instance.UpdateTask(context, resource);
        }
    }
}