// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Unity.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.Core.Scripting;

#if MOTIVE_VUFORIA
using Motive.AR.Vuforia;
#endif

namespace Motive.Unity.Scripting
{
    public class VisualMarker3DAssetProcessor : ThreadSafeScriptResourceProcessor<VisualMarker3DAsset>
    {
        public override void ActivateResource(ResourceActivationContext context, VisualMarker3DAsset resource)
        {
#if MOTIVE_VUFORIA
        VuforiaWorld.Instance.Add3dAsset(context, resource);
#endif
        }

        public override void DeactivateResource(ResourceActivationContext context, VisualMarker3DAsset resource)
        {
#if MOTIVE_VUFORIA
        VuforiaWorld.Instance.Remove3DAsset(context, resource);
#endif
        }
    }
}