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
    public class LocationAugmented3DAssetProcessor : ThreadSafeScriptResourceProcessor<LocationAugmented3DAsset>
    {
        public override void ActivateResource(ResourceActivationContext context, LocationAugmented3DAsset resource)
        {
            ARWorld.Instance.AddLocationAugmented3dAsset(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, LocationAugmented3DAsset resource)
        {
            ARWorld.Instance.RemoveLocationAugmented3dAsset(context.InstanceId);
        }
    }

}