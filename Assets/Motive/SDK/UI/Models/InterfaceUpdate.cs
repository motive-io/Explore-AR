// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Media;
using Motive.Core.Models;
using Motive.Core.Scripting;
using System.Collections.Generic;

namespace Motive.UI.Models
{
    /// <summary>
    /// Updates part of the app UI. Implementation of the update code
    /// is left to the developer.
    /// </summary>
	public class InterfaceUpdate : ScriptObject, IMediaItemProvider
    {
        public string Target { get; set; }
        public IContent Content { get; set; }

        public void GetMediaItems(IList<MediaItem> items)
        {
            var provider = Content as IMediaItemProvider;
            if (provider != null)
            {
                provider.GetMediaItems(items);
            }
        }
    }
}
