// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.AR.Models;
using Motive.Unity.Maps;

namespace Motive.Unity.Scripting
{
    public class LayeredMapOverlayProcessor : ThreadSafeScriptResourceProcessor<LayeredMapOverlay>
    {
        public override void ActivateResource(Motive.Core.Scripting.ResourceActivationContext context, LayeredMapOverlay resource)
        {
            MapController.Instance.AddLayeredOverlay(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, LayeredMapOverlay resource)
        {
            MapController.Instance.RemoveLayeredOverlay(context, resource);
        }
    }
}

