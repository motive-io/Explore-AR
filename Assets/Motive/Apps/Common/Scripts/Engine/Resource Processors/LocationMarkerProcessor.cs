// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Core.Scripting;
using Motive.Unity.Maps;
using Motive.Unity.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Scripting
{
    public class LocationMarkerProcessor : ThreadSafeScriptResourceProcessor<LocationMarker>
    {
        public override void ActivateResource(ResourceActivationContext context, LocationMarker resource)
        {
            if (MapMarkerAnnotationHandler.Instance)
            {
                MapMarkerAnnotationHandler.Instance.AddLocationMarker(context.InstanceId, resource);
            }

            base.ActivateResource(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, LocationMarker resource)
        {
            if (MapMarkerAnnotationHandler.Instance)
            {
                MapMarkerAnnotationHandler.Instance.RemoveLocationMarker(context.InstanceId);
            }

            base.DeactivateResource(context, resource);
        }

        public override void UpdateResource(ResourceActivationContext context, LocationMarker resource)
        {
            DeactivateResource(context, resource);
            ActivateResource(context, resource);

            base.UpdateResource(context, resource);
        }
    }
}