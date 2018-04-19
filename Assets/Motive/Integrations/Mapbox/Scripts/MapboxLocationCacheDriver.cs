// Copyright (c) 2018 RocketChicken Interactive Inc.

using Motive.Core.WebServices;
using System.Linq;
using System.Collections.Generic;
using Motive.AR.LocationServices;
using Motive.AR.Maps;
using Motive.Core.Models;
using Motive.AR.Models;
using Motive.Core.Utilities;

using Logger = Motive.Core.Diagnostics.Logger;
using System;
using Motive.Unity.Apps;
using Motive.Unity.Utilities;
using Motive.Unity.Storage;

#if MOTIVE_MAPBOX
using Mapbox.VectorTile;
using Mapbox.VectorTile.ExtensionMethods;
using Mapbox.Unity.Utilities;
using Motive.Core.Scripting;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Fills the location cache with POI data from Mapbox tiles. Maps the maki and OSM types
    /// to Motive location types.
    /// </summary>
    public class MapboxLocationCacheDriver : SingletonComponent<MapboxLocationCacheDriver>
    {
        public int TileZoomLevel = 16;
        public int TileSpan = 4;
        public int TileCacheSize = 32;
        public string TileSource = "mapbox.mapbox-streets-v7";

        class TileInfo
        {
            public int X;
            public int Y;
            public ServiceCall ServiceCall;
            public IEnumerable<Location> Locations;
        }

        TileInfo[,] m_tileInfos;

        int m_currMinX;
        int m_currMinY;

        Logger m_logger;

        LocationTypeMapper m_osmMapper;
        LocationTypeMapper m_makiMapper;
        StoryTagLocationTypeMap m_storyTagMap;
        LocationTracker m_locationTracker;

        string m_mapboxToken;
        TileCache m_tileCache;

        protected override void Awake()
        {
            base.Awake();

            m_logger = new Logger(this);

            m_tileInfos = new TileInfo[TileSpan, TileSpan];

            m_currMinX = -1;
            m_currMinY = -1;
        }

        public void Initialize(string mapboxToken, string storyTagCatalogName = null)
        {
            m_tileCache = new TileCache(StorageManager.EnsureCacheFolder("mapboxPoiTileCache"), TileCacheSize, TileCacheSize * 2, TimeSpan.FromDays(7));

            m_mapboxToken = mapboxToken;

            Catalog<LocationTypeMap> osmMap = null;
            Catalog<LocationTypeMap> makiMap = null;
            IEnumerable<StoryTagLocationType> storyTags = null;

            Motive.WebServices.Instance.AddCatalogLoad<LocationTypeMap>("motive.ar", "maki_location_type_map", (catalog) =>
            {
                makiMap = catalog;
            }, false);

            Motive.WebServices.Instance.AddCatalogLoad<LocationTypeMap>("motive.ar", "osm_location_type_map", (catalog) =>
            {
                osmMap = catalog;
            }, false);

            if (storyTagCatalogName != null)
            {
                Motive.WebServices.Instance.AddCatalogLoad<StoryTagLocationType>(storyTagCatalogName, (catalog) =>
                {
                    storyTags = catalog;
                });
            }

            AppManager.Instance.OnLoadComplete((callback) =>
            {
                m_osmMapper = new LocationTypeMapper(osmMap);
                m_makiMapper = new LocationTypeMapper(makiMap);

                if (storyTagCatalogName == null)
                {
                    storyTags = ScriptObjectDirectory.Instance.GetAllObjects<StoryTagLocationType>();
                }

                if (storyTags != null)
                {
                    m_storyTagMap = new StoryTagLocationTypeMap(storyTags);
                }

                m_locationTracker = UserLocationService.Instance.CreateLocationTracker(25);
                m_locationTracker.UpdateDistance = 25;

                m_locationTracker.Updated += t_Updated;

                m_locationTracker.Start();

                callback();
            });
        }

        void t_Updated(object sender, LocationUpdaterEventArgs e)
        {
            UpdateCache(e.Reading.Coordinates);
        }

        ServiceCall GetServiceCall(int x, int y, int z)
        {
            var url =
                string.Format("https://api.mapbox.com:443/v4/{0}/{1}/{2}/{3}.vector.pbf?access_token={4}",
                TileSource, z, x, y, m_mapboxToken);

            return new ServiceCall(url);
        }

        void UpdateCache(Coordinates origin)
        {
            var originTile = MapTools.GetTileVector(origin.Latitude, origin.Longitude, TileZoomLevel);

            var minX = (int)(originTile.X - TileSpan / 2 + 1);
            var minY = (int)(originTile.Y - TileSpan / 2 + 1);
            var maxX = minX + TileSpan;
            var maxY = minY + TileSpan;

            if (minX == m_currMinX && minY == m_currMinY)
            {
                return;
            }

            m_currMinX = minX;
            m_currMinY = minY;

            List<Location> toAdd = new List<Location>();
            List<Location> toRemove = new List<Location>();

            BatchProcessor iter = new BatchProcessor(TileSpan * TileSpan, () =>
            {
                LocationCache.Instance.UpdateCache(toAdd, toRemove);
            });

            for (int x = minX; x < minX + TileSpan; x++)
            {
                for (int y = minY; y < minY + TileSpan; y++)
                {
                    var ix = x % TileSpan;
                    var iy = y % TileSpan;

                    var info = m_tileInfos[ix, iy];

                    if (info == null || info.X != x || info.Y != y)
                    {
                        // Create a new one
                        if (info != null)
                        {
                            lock (info)
                            {
                                if (info.ServiceCall != null)
                                {
                                    info.ServiceCall.Abort();
                                }

                                info.ServiceCall = null;

                                if (info.Locations != null)
                                {
                                    lock (toRemove)
                                    {
                                        toRemove.AddRange(info.Locations);
                                    }
                                }
                            }
                        }
                        else
                        {
                            info = new TileInfo();
                        }

                        info.X = x;
                        info.Y = y;

                        m_tileInfos[ix, iy] = info;

                        var _info = info;
                        var _x = x;
                        var _y = y;

                        m_logger.Debug("Requesting locations for tile {0},{1},{2}",
                            x, y, TileZoomLevel);

                        Action<byte[]> processData = (data) =>
                        {
                            // Since we're sharing the infos, these values could change
                            lock (_info)
                            {
                                if (_info.X != _x || _info.Y != _y)
                                {
                                    return;
                                }

                                if (data != null)
                                {
                                    _info.Locations = GetLocations(data, _x, _y, TileZoomLevel);

                                    if (_info.Locations != null)
                                    {
                                        lock (toAdd)
                                        {
                                            toAdd.AddRange(_info.Locations);
                                        }
                                    }
                                }
                            }

                            iter++;
                        };

                        var cacheBytes = m_tileCache.GetTileData(TileSource, _x, _y, TileZoomLevel);

                        if (cacheBytes != null)
                        {
                            processData(cacheBytes);
                        }
                        else
                        {
                            var serviceCall = GetServiceCall(x, y, TileZoomLevel);
                            info.ServiceCall = serviceCall;

                            serviceCall.Connect(() =>
                            {
                                var data = serviceCall.ResponseBuffer;

                                if (data != null)
                                {
                                    processData(serviceCall.ResponseBuffer);

                                    m_tileCache.StoreTile(TileSource, _x, _y, TileZoomLevel, data);
                                }
                            });
                        }
                    }
                    else
                    {
                        iter++;
                    }
                }
            }
        }

        IEnumerable<Location> GetLocations(byte[] data, int x, int y, int z)
        {
            var tile = new Mapbox.Map.VectorTile();
            tile.ParseTileData(data);

            var layer = tile.Data.GetLayer("poi_label");

            m_logger.Debug("Parsing locations for tile {0},{1},{2}",
                x, y, TileZoomLevel);

            return GetLocations(layer, x, y, z);
        }

        IEnumerable<Location> GetLocations(VectorTileLayer layer, int x, int y, int z)
        {
            if (layer == null)
            {
                return null;
            }

            var fc = layer.FeatureCount();

            var locations = new List<Location>();

            for (int i = 0; i < fc; i++)
            {
                var feature = layer.GetFeature(i);
                var properties = feature.GetProperties();

                var points = feature.GeometryAsWgs84((ulong)z, (ulong)x, (ulong)y);

                //go.layer = LayerMask.NameToLayer(_key);
                if (!points.Any())
                    continue;

                int selpos = points[0].Count / 2;
                var met = Conversions.LatLonToMeters(points[0][selpos].Lat, points[0][selpos].Lng);

                if (!properties.ContainsKey("name"))
                    continue;

                var location = ConvertToLocation(feature, points, properties);

                locations.Add(location);
            }

            return locations;
        }


        Location ConvertToLocation(
            VectorTileFeature feature,
            List<List<Mapbox.VectorTile.Geometry.LatLng>> points, Dictionary<string, object> properties)
        {
            int selpos = points[0].Count / 2;

            var coords = new Coordinates(points[0][selpos].Lat, points[0][selpos].Lng);

            var loc = new Location(coords);
            loc.Name = properties["name"].ToString();

            loc.Id = "mapbox:" + feature.Id;

            List<string> types = new List<string>();

            if (properties.ContainsKey("maki"))
            {
                //var makiTypes = properties["maki"].ToString();
                var makiTypes = m_makiMapper.GetTypes(properties["maki"].ToString());

                if (makiTypes != null)
                {
                    types.AddRange(makiTypes);
                }
            }
            if (properties.ContainsKey("type"))
            {
                //var osmTypes = properties["type"].ToString();
                var osmTypes = m_osmMapper.GetTypes(properties["type"].ToString());

                if (osmTypes != null)
                {
                    types.AddRange(osmTypes);
                }
            }

            loc.LocationTypes = types.Distinct().ToArray();
            if (m_storyTagMap != null)
            {
                loc.StoryTags = m_storyTagMap.GetTagsForTypes(loc.LocationTypes);
            }

            return loc;
        }
    }
}
#endif