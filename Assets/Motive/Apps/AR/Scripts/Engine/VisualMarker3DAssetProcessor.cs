// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Core.Scripting;
using Motive.Unity.AR;

namespace Motive.Unity.Scripting
{
    public class VisualMarker3DAssetProcessor : ThreadSafeScriptResourceProcessor<VisualMarker3DAsset>
    {
        public override void ActivateResource(ResourceActivationContext context, VisualMarker3DAsset resource)
        {
            ARMarkerManager.Instance.Add3DAsset(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, VisualMarker3DAsset resource)
        {
            ARMarkerManager.Instance.Remove3DAsset(context, resource);
        }
    }
}