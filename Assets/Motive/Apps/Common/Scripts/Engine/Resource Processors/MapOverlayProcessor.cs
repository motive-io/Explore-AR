// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Core.Scripting;
using Motive.Unity.Maps;

namespace Motive.Unity.Scripting
{
    public class MapOverlayProcessor : ThreadSafeScriptResourceProcessor<MapOverlay>
    {
        public override void ActivateResource(ResourceActivationContext context, MapOverlay resource)
        {
            MapController.Instance.AddMapOverlay(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, MapOverlay resource)
        {
            MapController.Instance.RemoveMapOverlay(context, resource);
        }
    }
}
