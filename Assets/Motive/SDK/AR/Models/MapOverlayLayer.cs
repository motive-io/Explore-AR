using Motive.Core.Media;
using Motive.Core.Models;
using Motive.Core.Scripting;
using Motive.UI;
using System.Collections.Generic;

namespace Motive.AR.Models
{
    public class MapOverlayLayer : IMediaItemProvider
    {
        public MediaElement MediaElement { get; set; }
        public DoubleRange ZoomRange { get; set; }

        public void GetMediaItems(IList<MediaItem> items)
        {
            if (MediaElement != null)
            {
                MediaElement.GetMediaItems(items);
            }
        }
    }
}
