// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.AR.Models;
using Motive.Core.Diagnostics;
using Motive.Core.Scripting;
using Motive.Core.Utilities;
using Motive.Unity.UI;
using System;
using System.Linq;
using System.Collections.Generic;

using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Manages spawning location valuables based on locations in the location cache.
    /// </summary>
    public class LocationValuablesSpawnItemDriver : LocationSpawnItemDriver<LocationValuablesCollection>
    {
        LocationTriggerPool m_triggerPool;
        Dictionary<string, ResourceActivationContext> m_activationContexts;
        //DictionaryDictionary<string, string, LocationSpawnItemResults<LocationValuablesCollection>> m_computedResults;
        PerfCounters m_perf;
        Logger m_logger;

        public bool IsRunning { get; private set; }

        public LocationValuablesSpawnItemDriver()
        {
            // Defaults? These ultimately get ignored since we'll
            // only add triggers for items with ranges specified.
            m_perf = new PerfCounters();
            m_logger = new Logger(this);
            m_triggerPool = new LocationTriggerPool(0, 75);
            m_activationContexts = new Dictionary<string, ResourceActivationContext>();
            //m_computedResults = new DictionaryDictionary<string, string, LocationSpawnItemResults<LocationValuablesCollection>>();

            LocationValuablesCollectionManager.Instance.ValuablesAdded += Instance_ValuablesAdded;
            LocationValuablesCollectionManager.Instance.LocationsAdded += Instance_LocationsAdded;
            LocationValuablesCollectionManager.Instance.ValuablesRemoved += Instance_ValuablesRemoved;
            LocationValuablesCollectionManager.Instance.LocationsRemoved += Instance_LocationsRemoved;

            LocationCache.Instance.LocationsAdded += Locations_Added;
        }

        private void Locations_Added(object sender, LocationCacheEventArgs e)
        {
            UpdateLocations(e.Locations);
        }

        public void Collect(string instanceId, Location location, LocationValuablesCollection spawnItems, bool showReward = false)
        {
            if (showReward)
            {
                RewardManager.Instance.ShowRewards(spawnItems.ValuablesCollection);
            }

            TransactionManager.Instance.AddValuables(spawnItems.ValuablesCollection);

            base.Collect(instanceId, location);
        }

        protected override void AllItemsCollected(string instanceId, LocationValuablesCollection item)
        {
            LocationValuablesCollectionManager.Instance.AllItemsCollected(instanceId, item);
        }

        public void Start()
        {
            IsRunning = true;

            m_triggerPool.Start();

            UpdateFromCache();
        }

        public void Stop()
        {
            IsRunning = false;

            m_triggerPool.Stop();
        }

        string GetPoolRequestId(string instanceId, Location location)
        {
            return string.Format("{0}.{1}", instanceId, location.Id);
        }

        protected override void OnItemsSpawned(string instanceId, LocationSpawnItemResults<LocationValuablesCollection> result)
        {
            if (result.SpawnItem.CollectOptions != null &&
                result.SpawnItem.CollectOptions.CollectRange != null)
            {
                m_triggerPool.WatchLocation(GetPoolRequestId(instanceId, result.SourceLocation), result.SourceLocation, () =>
                    {
                        ResourceActivationContext ctxt;

                        if (m_activationContexts.TryGetValue(instanceId, out ctxt))
                        {
                            ctxt.FireEvent("in_range");
                        }

                    }, result.SpawnItem.CollectOptions.CollectRange);
            }

            base.OnItemsSpawned(instanceId, result);
        }

        protected override void OnItemsRemoved(string instanceId, LocationSpawnItemResults<LocationValuablesCollection> result)
        {
            m_triggerPool.StopWatching(GetPoolRequestId(instanceId, result.SourceLocation));

            base.OnItemsRemoved(instanceId, result);
        }

        private void Instance_LocationsRemoved(object sender, LocationValuablesCollectionManagerEventArgs e)
        {
        }

        private void Instance_ValuablesRemoved(object sender, LocationValuablesCollectionManagerEventArgs e)
        {
            foreach (var lvc in e.LocationValuables)
            {
                var spawnLocations = GetLocationsForSpawnItem(lvc.ActivationContext.InstanceId);

                if (spawnLocations != null)
                {
                    foreach (var location in spawnLocations)
                    {
                        m_triggerPool.StopWatching(GetPoolRequestId(lvc.ActivationContext.InstanceId, location));
                    }
                }

                m_activationContexts.Remove(lvc.ActivationContext.InstanceId);

                //m_computedResults.RemoveAll(lvc.ActivationContext.InstanceId);

                RemoveSpawnItem(lvc.ActivationContext.InstanceId, lvc.SpawnItem, true);
            }
        }

        private void Instance_LocationsAdded(object sender, LocationValuablesCollectionManagerEventArgs e)
        {
        }

        private void Instance_ValuablesAdded(object sender, LocationValuablesCollectionManagerEventArgs e)
        {
            foreach (var lvc in e.LocationValuables)
            {
                AddSpawnItem(lvc.ActivationContext.InstanceId, lvc.SpawnItem);

                m_activationContexts[lvc.ActivationContext.InstanceId] = lvc.ActivationContext;
            }

            ProcessValuables(e.LocationValuables);
        }

        void ProcessValuables(IEnumerable<LocationValuablesCollectionItemContainer> locationValuables)
        {
            m_perf = new PerfCounters();

            m_perf.Start("v");

            // Group items by the sets of locations they reference.
            // The goal here is to reduce the number of locations we visit, ideally as close to 1
            // visit per location as possible.
            SetDictionary<IEnumerable<Location>, LocationValuablesCollectionItemContainer> fixedSets =
                new SetDictionary<IEnumerable<Location>, LocationValuablesCollectionItemContainer>();

            // Location sets per story tag
            Dictionary<string, IEnumerable<Location>> tagSets = new Dictionary<string, IEnumerable<Location>>();
            // Location sets per location type
            Dictionary<string, IEnumerable<Location>> typeSets = new Dictionary<string, IEnumerable<Location>>();

            SetDictionary<string, LocationValuablesCollectionItemContainer> tagLvcs =
                new SetDictionary<string, LocationValuablesCollectionItemContainer>();

            SetDictionary<string, LocationValuablesCollectionItemContainer> typeLvcs =
                new SetDictionary<string, LocationValuablesCollectionItemContainer>();

            foreach (var lvc in locationValuables)
            {
                if (lvc.SpawnItem.Locations != null)
                {
                    fixedSets.Add(lvc.SpawnItem.Locations, lvc);
                }

                if (lvc.SpawnItem.LocationTypes != null)
                {
                    foreach (var type in lvc.SpawnItem.LocationTypes)
                    {
                        if (!typeSets.ContainsKey(type))
                        {
                            typeSets[type] = LocationCache.Instance.GetLocationsWithType(type);
                        }

                        typeLvcs.Add(type, lvc);
                    }
                }

                if (lvc.SpawnItem.StoryTags != null)
                {
                    foreach (var tag in lvc.SpawnItem.StoryTags)
                    {
                        if (!tagSets.ContainsKey(tag))
                        {
                            tagSets[tag] = LocationCache.Instance.GetLocationsWithStoryTag(tag);
                        }

                        tagLvcs.Add(tag, lvc);
                    }
                }
            }

            Dictionary<string, Location> processedLocations = new Dictionary<string, Location>();

            // Processes the set of locations, optionally creating a union with an existing set
            Action<IEnumerable<Location>, IEnumerable<LocationValuablesCollectionItemContainer>> processLocations =
                (locs, lvcsIn) =>
            {
                m_perf.Start("l");

                m_perf.Start("u");

                HashSet<LocationValuablesCollectionItemContainer> lvcs = new HashSet<LocationValuablesCollectionItemContainer>();

                if (lvcsIn != null)
                {
                    lvcs.UnionWith(lvcsIn);
                }

                m_perf.Stop("u");

                m_perf.Start("locs");

                if (locs != null)
                {
                    foreach (var location in locs)
                    {
                        if (!processedLocations.ContainsKey(location.Id))
                        {
                            processedLocations.Add(location.Id, location);

                            if (location.LocationTypes != null)
                            {
                                foreach (var type in location.LocationTypes)
                                {
                                    var tl = typeLvcs[type];

                                    if (tl != null)
                                    {
                                        lvcs.UnionWith(tl);
                                    }
                                }
                            }

                            if (location.StoryTags != null)
                            {
                                foreach (var tag in location.StoryTags)
                                {
                                    var tl = tagLvcs[tag];

                                    if (tl != null)
                                    {
                                        lvcs.UnionWith(tl);
                                    }
                                }
                            }

                            m_perf.Start("c");

                            // Can be smarter here: if LVCs haven't changed, only locations, then
                            // any location that's been checked can be skipped
                            foreach (var lvc in lvcs)
                            {
                                // Have we processed this lvc/location combo recently??
                                // Note: this currently persists until the resource is deactivated.
                                LocationSpawnItemResults<LocationValuablesCollection> results = null;

                                lock (m_locationComputedItems)
                                {
                                    results = GetResults(lvc.ActivationContext.InstanceId, location);

                                    if (results != null)
                                    {
                                        continue;
                                    }

                                    results =
                                        new LocationSpawnItemResults<LocationValuablesCollection>(lvc.ActivationContext.InstanceId, location, lvc.SpawnItem);

                                    var prob = lvc.SpawnItem.Probability.GetValueOrDefault(1);
                                    var rnd = Tools.RandomDouble(0, 1);

                                    if (rnd < prob)
                                    {
                                        // Tada!
                                        results.DidSpawn = true;
                                    }

                                    AddComputedResult(location, lvc.InstanceId, results);
                                }

                                if (results.DidSpawn)
                                {
                                    OnItemsSpawned(lvc.InstanceId, results);
                                }
                            }

                            m_perf.Stop("c");
                        }
                        else
                        {
                            // TODO: locations re-used
                        }
                    }
                }

                m_perf.Stop("locs");

                m_perf.Stop("l");
            };

            foreach (var set in fixedSets.Keys)
            {
                processLocations(set, fixedSets[set]);
            }

            foreach (var set in tagSets.Values)
            {
                processLocations(set, null);
            }

            foreach (var set in typeSets.Values)
            {
                processLocations(set, null);
            }

            /*
            m_activationContexts.Add(e.ActivationContext.InstanceId, e.ActivationContext);
            */

            //if (IsRunning)
            {
                //AddSpawnItems(e.LocationValuables, null/*e.ActivationContext.GetStorageAgent()*/);
            }

            m_perf.Stop("v");

            m_logger.Debug("ProcessValuables perf={0}", m_perf.ToShortString());
        }

        void UpdateLocations(IEnumerable<Location> locations)
        {
            // TODO: We could make this more efficient by only restricting to only "locations"
            // that have been updated.
            ProcessValuables(LocationValuablesCollectionManager.Instance.AllValuables);
        }

        private void UpdateFromCache()
        {
            if (IsRunning)
            {
                UpdateLocations(LocationCache.Instance.Locations);
            }
        }

        private void Cache_Updated(object sender, EventArgs e)
        {
            UpdateFromCache();
        }
    }

}