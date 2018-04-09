// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.Gaming.Models;
using Motive.Core.Scripting;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class ObjectiveActivatorProcessor : ScriptResourceProcessor<ObjectiveActivator>
    {
        public override void ActivateResource(ResourceActivationContext context, ObjectiveActivator resource)
        {
            if (resource.Objective != null && !context.IsClosed)
            {
                TaskManager.Instance.ActivateObjective(resource.Objective);

                context.Close();
            }
        }
    }
}