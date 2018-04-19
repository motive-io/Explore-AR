using System;
using System.Collections;
using System.Collections.Generic;
using Motive.Unity.Apps;
using UnityEngine;

#if MOTIVE_MAPBOX
using Mapbox.Unity;
#endif

namespace Motive.Unity.Maps
{
    public class MapboxInitializer : Initializer
    {
#if MOTIVE_MAPBOX
        protected override void Initialize()
        {
            if (!string.IsNullOrEmpty(MapboxAccess.Instance.Configuration.AccessToken) &&
                MapboxLocationCacheDriver.Instance != null)
            {
                MapboxLocationCacheDriver.Instance.Initialize(MapboxAccess.Instance.Configuration.AccessToken);

                var source = GameObject.FindObjectOfType<MapBoxSource>();

                if (source)
                {
                    source.AccessToken = MapboxAccess.Instance.Configuration.AccessToken;
                }
            }
        }
#endif
    }
}