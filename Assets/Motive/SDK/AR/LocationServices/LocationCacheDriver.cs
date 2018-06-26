// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Scripting;
using Motive.Core.Diagnostics;
using Motive.Core.Models;
using Motive.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.LocationServices
{
    public class LocationCacheSearchFilter
    {
        public string[] LocationTypes { get; set; }
        public string[] StoryTags { get; set; }
        public int Range { get; set; }
    }

    public class LocationCacheDriver : Singleton<LocationCacheDriver>
    {
        public int SearchMoveDistance = 100;
        public int SearchRange = 1000;

        private Coordinates m_lastCoordinates;
        private ILocationSearchProvider m_searchProvider;
        private ILocationSearch m_search;

        public bool IsRunning { get; private set; }

        // Add some searches specifically for the story tags we're interested in
        // to augment the location cache.
        private List<ILocationSearch> m_storyTagSearches;
        //private Catalog<Location> m_locationCatalog;

        Logger m_logger;
        bool m_initialized;
        LocationTracker m_locationTracker;

        public bool CacheReady { get; private set; }

        //public event EventHandler Updated;
        
        public void Initialize()
        {
            if (m_initialized) return;

            m_initialized = true;

            m_logger = new Logger(this);

            m_locationTracker = UserLocationService.Instance.CreateLocationTracker(SearchMoveDistance / 2);
            m_locationTracker.Updated += LocationTracker_Updated;

            m_logger.Debug("Initialize");            
        }

        public void AddSearchProvider(ILocationSearchProvider searchProvider)
        {
            // Todo: allow multiple search providers - or caller needs to create
            // an aggregate provider?
            m_searchProvider = searchProvider;
        }

        public void AddFilter(LocationCacheSearchFilter filter)
        {
            /// Clear cache and apply filter
        }

        public void ClearFilters()
        {
            /// Reset filters and do search
        }

        // Not used currently - this allows apps to set up "special" locations
        // that get an extra search to add some flavour. Locations that might
        // not show up in a browse search.
        void DoStoryTagSearch(Action onDone, params string[] storyTags)
        {
            var search = m_searchProvider.CreateSearch();

            lock (m_storyTagSearches)
            {
                m_storyTagSearches.Add(search);
            }

            search.SearchStoryTags(
                m_locationTracker.LastReading.Coordinates,
                storyTags,
                SearchRange,
                5 * storyTags.Length,
                () =>
                {
                    if (search.Locations != null)
                    {
                        LocationCache.Instance.AddLocationsToCache(search.Locations);
                    }

                    lock (m_storyTagSearches)
                    {
                        m_storyTagSearches.Remove(search);
                        if (m_storyTagSearches.Count == 0)
                        {
                            onDone();
                        }
                    }
                });
        }

        void SearchCompleted()
        {
            m_logger.Debug("Completed search");

            /*
             * If you set m_locationCatalog, any locations in the specified
             * catalog will be added to the cache.
            if (m_locationCatalog != null)
            {
                LocationCache.Instance.SetCachedLocations(m_locationCatalog);
                LocationCache.Instance.AddLocationsToCache(m_search.Locations);
            }
            else
            {
            }*/
            LocationCache.Instance.SetCachedLocations(m_search.Locations);
        }

        private void DoSearch(Coordinates coordinates)
        {
            IEnumerable<ILocationSearch> toCancel = null;

            /*
            lock (m_storyTagSearches)
            {
                toCancel = m_storyTagSearches;
                m_storyTagSearches = new List<ILocationSearch>();
            }*/

            if (toCancel != null)
            {
                foreach (var s in toCancel)
                {
                    s.Abort();
                }
            }

            m_search.Search(coordinates, SearchRange, SearchCompleted);
        }

        void HandleSystemPositionUpdated(Coordinates coordinates)
        {
            if (ShouldSearch(coordinates))
            {
                m_logger.Verbose("HandleSystemPositionUpdated - ShouldSearch: true");

                m_lastCoordinates = coordinates;

                if (m_search != null)
                {
                    m_search.Abort();
                    m_search = null;
                }

                m_search = m_searchProvider.CreateSearch();

                m_logger.Verbose("Searching");

                DoSearch(coordinates);
            }
        }

        bool ShouldSearch(Coordinates coords)
        {
            if (m_lastCoordinates != null)
            {
                return m_lastCoordinates.GetDistanceFrom(coords) > SearchMoveDistance;
            }

            return true;
        }

        void LocationTracker_Updated(object sender, LocationUpdaterEventArgs e)
        {
            HandleSystemPositionUpdated(e.Reading.Coordinates);
        }

        public void Start()
        {
            if (!IsRunning)
            {
                m_locationTracker.Start();

                IsRunning = true;
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                m_locationTracker.Stop();

                IsRunning = false;
            }
        }
    }
}
