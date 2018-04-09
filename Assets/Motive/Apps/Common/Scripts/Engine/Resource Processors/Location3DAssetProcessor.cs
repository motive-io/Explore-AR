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
    public class Location3DAssetProcessor : ThreadSafeScriptResourceProcessor<Location3DAsset>
    {
        public override void ActivateResource(ResourceActivationContext context, Location3DAsset resource)
        {
            MapMarkerAnnotationHandler.Instance.AddLocation3DAsset(resource);

            base.ActivateResource(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, Location3DAsset resource)
        {
            MapMarkerAnnotationHandler.Instance.RemoveLocation3DAsset(resource);

            base.DeactivateResource(context, resource);
        }
    }

}