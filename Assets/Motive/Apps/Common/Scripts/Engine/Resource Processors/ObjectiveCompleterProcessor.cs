// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class ObjectiveCompleterProcessor : ScriptResourceProcessor<ObjectiveCompleter>
    {
        public override void ActivateResource(ResourceActivationContext context, ObjectiveCompleter resource)
        {
            if (resource.Objective != null && !context.IsClosed)
            {
                TaskManager.Instance.CompleteObjective(resource.Objective);

                context.Close();
            }
        }
    }
}