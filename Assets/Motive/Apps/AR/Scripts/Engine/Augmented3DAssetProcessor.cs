// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Core.Scripting;
using Motive.Unity.AR;
using Motive.Unity.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Scripting
{
    public class Augmented3DAssetProcessor : ThreadSafeScriptResourceProcessor<Augmented3DAsset>
    {
        public override void ActivateResource(ResourceActivationContext context, Augmented3DAsset resource)
        {
            ARWorld.Instance.AddAugmented3dAsset(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, Augmented3DAsset resource)
        {
            ARWorld.Instance.RemoveAugmented3dAsset(context.InstanceId);
        }
    }
}