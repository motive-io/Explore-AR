// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Scripting;
using Motive.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive._3D.Models
{
    /// <summary>
    /// Defines an instance of an asset, including intial layout parameters.
    /// </summary>
    public class AssetInstance : ScriptObject, IMediaItemProvider
    {
        public IScriptObject Asset { get; set; }
        public Layout Layout { get; set; }

        public void GetMediaItems(IList<Core.Media.MediaItem> items)
        {
            var provider = Asset as IMediaItemProvider;

            if (provider != null)
            {
                provider.GetMediaItems(items);
            }
        }
    }
}
