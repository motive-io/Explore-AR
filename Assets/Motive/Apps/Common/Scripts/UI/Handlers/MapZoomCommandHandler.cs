using Motive.AR.Models;
using Motive.Core.Scripting;
using Motive.Unity.Maps;
using System;

namespace Motive.Unity.UI
{
    public class MapZoomCommandHandler : InterfaceCommandHandler<MapZoomCommand>
    {
        public float DefaultZoomDuration = 0.6f;

        public override void ProcessInterfaceCommand(ResourceActivationContext context, MapZoomCommand mapZoom, Action onComplete)
        {
            if (mapZoom.CenterLocation != null)
            {
                MapController.Instance.PanZoomTo(
                    mapZoom.CenterLocation.Coordinates,
                    mapZoom.Span,
                    (float)mapZoom.Duration.GetValueOrDefault(TimeSpan.FromSeconds(DefaultZoomDuration)).TotalSeconds,
                    () =>
                    {
                        // Select the annotation at the location if it's set
                        MapController.Instance.SelectLocation(mapZoom.CenterLocation);

                        onComplete();
                    });
            }
        }

        private void Awake()
        {
            RegisterType("motive.ar.mapZoomCommand");
        }
    }
}
