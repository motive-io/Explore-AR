// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Unity.Gaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Scripting
{
    public class AssignmentProcessor : ScriptResourceProcessor<Assignment>
    {
        public override void ActivateResource(ResourceActivationContext context, Assignment resource)
        {
            TaskManager.Instance.ActivateAssignment(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, Assignment resource)
        {
            TaskManager.Instance.DeactivateAssignment(context, resource);
        }
    }
}