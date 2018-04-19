// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.AR.Models;
using Motive.Core.Utilities;
using Motive.Unity.Gaming;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Motive.Unity.Gaming
{
    public class LocationSpawnItemResults<T>
        where T : ILocationSpawnItemOptions
    {
        public string InstanceId { get; private set; }
        public Location SourceLocation { get; private set; }
        public T SpawnItem { get; private set; }
        public bool DidSpawn { get; set; }
        public bool DidCollect { get; set; }

        public LocationSpawnItemResults(
            string instanceId, Location sourceLocation, T spawnItem)
        {
            this.InstanceId = instanceId;
            this.SourceLocation = sourceLocation;
            this.SpawnItem = spawnItem;
        }
    }

    public class LocationSpawnItemContainer<T>
        where T : ILocationSpawnItemOptions
    {
        public T SpawnItem { get; private set; }
        public string InstanceId { get; private set; }

        public LocationSpawnItemContainer(string instanceId, T item)
        {
            InstanceId = instanceId;
            SpawnItem = item;
        }
    }

    public class LocationSpawnItemDriverEventArgs<T> : EventArgs
        where T : ILocationSpawnItemOptions
    {
        public LocationSpawnItemResults<T> Results { get; private set; }

        internal LocationSpawnItemDriverEventArgs(LocationSpawnItemResults<T> results)
        {
            this.Results = results;
        }
    }

}
/// <summary>
/// This driver handles spawning items at locations based on the given options.
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class LocationSpawnItemDriver<T> where T : ILocationSpawnItemOptions
{
    public event EventHandler<LocationSpawnItemDriverEventArgs<T>> ItemsSpawned;
    public event EventHandler<LocationSpawnItemDriverEventArgs<T>> ItemsRemoved;

    //private Logger m_logger;
    //private PerfCounters m_perfCounters;

    private Dictionary<string, Location> m_waitingLocations;
    //private Dictionary<string, Location> m_usedLocations;

    // Every location has a set of 
    protected class ComputedLocationSpawnItems
    {
        public Dictionary<string, LocationSpawnItemResults<T>> Results;

        public ComputedLocationSpawnItems()
        {
            Results = new Dictionary<string, LocationSpawnItemResults<T>>();
        }
    }

    public class SpawnState
    {
        public DateTime SpawnTime { get; set; }
    }

    protected Dictionary<string, ComputedLocationSpawnItems> m_locationComputedItems;
    Dictionary<string, T> m_spawnItems;
    SetDictionary<string, Location> m_itemLocations;

    public LocationSpawnItemDriver()
    {
        //m_logger = new Logger(this);
        //m_perfCounters = new PerfCounters();

        m_locationComputedItems = new Dictionary<string, ComputedLocationSpawnItems>();
        m_itemLocations = new SetDictionary<string, Location>();
        m_spawnItems = new Dictionary<string, T>();

        m_itemLocations = new SetDictionary<string, Location>();
        //m_usedLocations = new Dictionary<string, Location>();
    }

    public LocationSpawnItemResults<T> GetResults(string instanceId, Location location)
    {
        ComputedLocationSpawnItems computed = null;

        // No hits for this item at this location
        if (m_locationComputedItems.TryGetValue(location.Id, out computed))
        {
            LocationSpawnItemResults<T> results;

            computed.Results.TryGetValue(instanceId, out results);

            return results;
        }

        return null;
    }

    protected ComputedLocationSpawnItems GetComputedItems(Location location)
    {
        ComputedLocationSpawnItems computed = null;

        m_locationComputedItems.TryGetValue(location.Id, out computed);

        return computed;
    }

    protected ComputedLocationSpawnItems GetOrCreateLocationComputedItems(Location location)
    {
        ComputedLocationSpawnItems computed = null;

        if (!m_locationComputedItems.TryGetValue(location.Id, out computed))
        {
            computed = new ComputedLocationSpawnItems();
            m_locationComputedItems.Add(location.Id, computed);
        }

        return computed;
    }

    protected void RemoveComputedResult(Location location, string instanceId)
    {
        var computed = GetComputedItems(location);

        if (computed != null)
        {
            computed.Results.Remove(instanceId);
        }
    }
    
    protected void AddComputedResult(Location location, string instanceId, LocationSpawnItemResults<T> results)
    {
        var computed = GetOrCreateLocationComputedItems(location);

        computed.Results.Add(instanceId, results);
    }

    // Remove items 
    public virtual void Remove(string instanceId, Location location, bool resetState = false, bool didCollect = false)
    {
        ComputedLocationSpawnItems computed = null;

        lock (m_itemLocations)
        {
            m_itemLocations.Remove(instanceId, location);
        }

        // No hits for this item at this location
        if (!m_locationComputedItems.TryGetValue(location.Id, out computed))
        {
            return;
        }

        LocationSpawnItemResults<T> result = null;

        if (!computed.Results.TryGetValue(instanceId, out result))
        {
            return;
        }

        result.DidCollect = didCollect;

        if (resetState)
        {
            computed.Results.Remove(instanceId);
        }

		OnItemsRemoved(instanceId, result);
    }

    // Collect specified items. By default simply removes them. Subclasses may have additional
    // logic relating to collect.
    public virtual void Collect(string instanceId, Location location)
    {
        Remove(instanceId, location, didCollect: true);

        int count = 0;

        lock (m_itemLocations)
        {
            count = m_itemLocations.GetCount(instanceId);
        }

        if (count == 0)
        {
            T item;

            if (m_spawnItems.TryGetValue(instanceId, out item))
            {
                AllItemsCollected(instanceId, item);
            }
        }
    }

    protected virtual void AllItemsCollected(string instanceId, T item)
    {
    }

	protected virtual void OnItemsRemoved(string instanceId, LocationSpawnItemResults<T> result)
	{
		if (ItemsRemoved != null)
		{
			ItemsRemoved(this, new LocationSpawnItemDriverEventArgs<T>(result));
		}
	}

	protected virtual void OnItemsSpawned(string instanceId, LocationSpawnItemResults<T> result)
	{
        m_itemLocations.Add(instanceId, result.SourceLocation);

		if (ItemsSpawned != null)
		{
			ItemsSpawned(this, new LocationSpawnItemDriverEventArgs<T>(result));
		}
	}

    protected IEnumerable<Location> GetLocationsForSpawnItem(string instanceId)
    {
        return m_itemLocations[instanceId];
    }

    protected void AddSpawnItem(string instanceId, T item)
    {
        m_spawnItems[instanceId] = item;
    }

    protected void RemoveSpawnItem(string instanceId, T item, bool resetState = false)
    {
        var locs = m_itemLocations[instanceId];

        if (locs != null)
        {
            foreach (var loc in locs.ToArray())
            {
                Remove(instanceId, loc, resetState);
            }
        }

        m_spawnItems.Remove(instanceId);
    }
}
