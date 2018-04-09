// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive;
using Motive.AR.LocationServices;
using Motive.Core.Models;
using Motive.Unity.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Apps
{
    /// <summary>
    /// Helper methods for app setup.
    /// </summary>
    public static partial class SetupHelper
    {
        /// <summary>
        /// Sets up foursquare.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="secret">The secret.</param>
        /// <param name="locationStoryTagMap">The location story tag map.</param>
        public static void SetUpFoursquare(string clientId, string secret, string locationStoryTagMap)
        {
            Catalog<FoursquareCategoryMap> categoryMap = null;
            Catalog<StoryTagLocationType> storyTags = null;

            WebServices.Instance.AddCatalogLoad<FoursquareCategoryMap>("motive.ar", "foursquare_category_map", (catalog) =>
            {
                categoryMap = catalog;
            }, false);

            WebServices.Instance.AddCatalogLoad<StoryTagLocationType>(locationStoryTagMap, (catalog) =>
            {
                storyTags = catalog;
            });

            AppManager.Instance.OnLoadComplete((callback) =>
            {
            // Use Foursquare - we can add to this.
            FoursquareService.Instance.Initialize(
                    clientId,
                    secret,
                    categoryMap,
                    storyTags,
                    (success) =>
                    {
                        if (success)
                        {
                            LocationCacheDriver.Instance.AddSearchProvider(FoursquareService.Instance.GetSearchProvider(FoursquareSearchIntent.Browse));

                            LocationCacheDriver.Instance.Start();
                        }

                        callback();
                    });
            });
        }

        /// <summary>
        /// Sets up defaults for Location-AR apps.
        /// </summary>
        internal static void SetUpLocationARDefaults()
        {
            Platform.Instance.Initialize();

            WebServices.Instance.Initialize();

            ScriptExtensions.Initialize(
                Platform.Instance.LocationManager,
                Platform.Instance.BeaconManager,
                ForegroundPositionService.Instance.Compass);

            //ARScriptExtensions.Initialize();

            DebugPlayerLocation.Instance.Initialize();
            ForegroundPositionService.Instance.Initialize();
        }
    }
}