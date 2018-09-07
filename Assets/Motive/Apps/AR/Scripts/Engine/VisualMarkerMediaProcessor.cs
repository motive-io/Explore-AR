// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Unity.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.Core.Scripting;
using Motive.Unity.AR;


namespace Motive.Unity.Scripting
{
    public class VisualMarkerMediaProcessor : ThreadSafeScriptResourceProcessor<VisualMarkerMedia>
    {
        public override void ActivateResource(ResourceActivationContext context, VisualMarkerMedia resource)
        {
            ARMarkerManager.Instance.AddMarkerMedia(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, VisualMarkerMedia resource)
        {
            ARMarkerManager.Instance.RemoveMarkerMedia(context, resource);
        }
    }
}