// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.UI.Models;
using Motive.Unity.Scripting;
using Motive.Unity.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Scripting
{
    public class ObjectInspectorProcessor : ThreadSafeScriptResourceProcessor<ObjectInspector>
    {
        public override void ActivateResource(ResourceActivationContext context, ObjectInspector resource)
        {
            ObjectInspectorManager.Instance.ActivateInspector(context, resource);

            base.ActivateResource(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, ObjectInspector resource)
        {
            ObjectInspectorManager.Instance.DeactivateInspector(context, resource);

            base.DeactivateResource(context, resource);
        }
    }
}