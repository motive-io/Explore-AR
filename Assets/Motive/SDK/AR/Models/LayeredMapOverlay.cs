using Motive.AR.LocationServices;
using Motive.Core.Media;
using Motive.Core.Models;
using Motive.Core.Scripting;
using System.Collections.Generic;

namespace Motive.AR.Models
{
    public class LayeredMapOverlay : ScriptObject, IMediaItemProvider
    {
        public LayeredMapOverlay()
        {
        }

        public Vector Size { get; set; }

        public Location CenterLocation { get; set; }

        public MapOverlayLayer[] MapOverlayLayers { get; set; }

        public void GetMediaItems(IList<MediaItem> items)
        {
            if (MapOverlayLayers != null)
            {
                foreach (var layer in MapOverlayLayers)
                {
                    layer.GetMediaItems(items);
                }
            }
        }
    }
}