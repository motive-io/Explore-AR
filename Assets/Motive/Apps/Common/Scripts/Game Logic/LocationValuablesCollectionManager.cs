// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.AR.LocationServices;
using Motive.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.Core.Scripting;

namespace Motive.Unity.Gaming
{
    public class LocationValuablesCollectionItemContainer
        : LocationSpawnItemContainer<LocationValuablesCollection>
    {
        public ResourceActivationContext ActivationContext { get; private set; }

        public LocationValuablesCollectionItemContainer(ResourceActivationContext context, LocationValuablesCollection lvc)
            : base(context.InstanceId, lvc)
        {
            ActivationContext = context;
        }
    }

    public class LocationValuablesCollectionManagerEventArgs : EventArgs
    {
        public IEnumerable<LocationValuablesCollectionItemContainer> LocationValuables { get; private set; }
        public IEnumerable<Location> Locations { get; private set; }
        public bool ResetState { get; private set; }

        public LocationValuablesCollectionManagerEventArgs(ResourceActivationContext ctxt, LocationValuablesCollection lvc, bool resetState = false)
        {
            this.ResetState = resetState;
            LocationValuables = new LocationValuablesCollectionItemContainer[]
            {
            new LocationValuablesCollectionItemContainer(ctxt, lvc)
            };
        }

        public LocationValuablesCollectionManagerEventArgs(IEnumerable<LocationValuablesCollectionItemContainer> lvcs, bool resetState = false)
        {
            this.ResetState = resetState;
            //this.Locations = lvcs.Locations;
            this.LocationValuables = lvcs;
        }

        public LocationValuablesCollectionManagerEventArgs(Location[] locations)
        {
            this.Locations = locations;
        }
    }

    public class LocationValuablesCollectionManager : Singleton<LocationValuablesCollectionManager>
    {
        public class LocationValuablesInstance
        {
            public ResourceActivationContext ActivationContext { get; set; }
            public Location Location { get; set; }
            public LocationValuablesCollection ValuablesCollection { get; set; }
        }

        Dictionary<string, ResourceActivationContext> m_contexts;
        SetDictionary<string, LocationValuablesInstance> m_valuablesByLocation;
        SetDictionary<string, LocationValuablesInstance> m_valuablesByType;
        SetDictionary<string, LocationValuablesInstance> m_valuablesByTag;

        bool m_updating;

        Dictionary<string, LocationValuablesCollectionItemContainer> m_addedValuables;
        Dictionary<string, LocationValuablesCollectionItemContainer> m_removedValuables;
        Dictionary<string, Location> m_addedLocations;
        Dictionary<string, Location> m_removedLocations;

        Dictionary<string, LocationValuablesCollectionItemContainer> m_allValuables;

        public event EventHandler<LocationValuablesCollectionManagerEventArgs> ValuablesAdded;
        public event EventHandler<LocationValuablesCollectionManagerEventArgs> ValuablesRemoved;

        public event EventHandler<LocationValuablesCollectionManagerEventArgs> LocationsAdded;
        public event EventHandler<LocationValuablesCollectionManagerEventArgs> LocationsRemoved;

        public IEnumerable<LocationValuablesCollectionItemContainer> AllValuables
        {
            get
            {
                return m_allValuables.Values.ToArray();
            }
        }

        public LocationValuablesCollectionManager()
        {
            m_valuablesByLocation = new SetDictionary<string, LocationValuablesInstance>();
            m_valuablesByType = new SetDictionary<string, LocationValuablesInstance>();
            m_valuablesByTag = new SetDictionary<string, LocationValuablesInstance>();
            m_contexts = new Dictionary<string, ResourceActivationContext>();

            m_addedLocations = new Dictionary<string, Location>();
            m_removedLocations = new Dictionary<string, Location>();
            m_addedValuables = new Dictionary<string, LocationValuablesCollectionItemContainer>();
            m_removedValuables = new Dictionary<string, LocationValuablesCollectionItemContainer>();
            m_allValuables = new Dictionary<string, LocationValuablesCollectionItemContainer>();
        }

        public void AddLocationValuablesCollection(ResourceActivationContext context, LocationValuablesCollection lvc)
        {
            if (context.IsClosed)
            {
                return;
            }

            context.Open();

            m_contexts.Add(context.InstanceId, context);

            List<Location> addedLocations = null;

            lock (m_valuablesByLocation)
            {
                var typeTagContainer = new LocationValuablesInstance
                {
                    ActivationContext = context,
                    ValuablesCollection = lvc
                };

                if (lvc.LocationTypes != null)
                {
                    foreach (var t in lvc.LocationTypes)
                    {
                        m_valuablesByType.Add(t, typeTagContainer);
                    }
                }

                if (lvc.StoryTags != null)
                {
                    foreach (var t in lvc.StoryTags)
                    {
                        m_valuablesByTag.Add(t, typeTagContainer);
                    }
                }
            }

            bool isUpdating;

            var container = new LocationValuablesCollectionItemContainer(context, lvc);
            m_allValuables.Add(context.InstanceId, container);

            lock (this)
            {
                isUpdating = m_updating;

                if (isUpdating)
                {
                    m_addedValuables.Add(context.InstanceId, container);
                    m_removedValuables.Remove(context.InstanceId);
                }
            }

            if (!isUpdating)
            {
                if (ValuablesAdded != null)
                {
                    ValuablesAdded(this, new LocationValuablesCollectionManagerEventArgs(context, lvc));
                }

                if (addedLocations != null && LocationsAdded != null)
                {
                    LocationsAdded(this, new LocationValuablesCollectionManagerEventArgs(addedLocations.ToArray()));
                }
            }
        }

        public void RemoveLocationValuablesCollection(ResourceActivationContext context, LocationValuablesCollection lvc, bool resetState = false)
        {
            List<Location> removedLocations = null;

            m_contexts.Remove(context.InstanceId);
            m_allValuables.Remove(context.InstanceId);

            lock (m_valuablesByLocation)
            {
                if (lvc.Locations != null)
                {
                    foreach (var l in lvc.Locations)
                    {
                        m_valuablesByLocation.RemoveWhere(l.Id, c => c.ActivationContext.InstanceId == context.InstanceId);

                        if (m_valuablesByLocation[l.Id] == null || m_valuablesByLocation[l.Id].Count() == 0)
                        {
                            if (removedLocations == null)
                            {
                                removedLocations = new List<Location>();
                            }

                            removedLocations.Add(l);
                        }
                    }
                }

                if (lvc.LocationTypes != null)
                {
                    foreach (var t in lvc.LocationTypes)
                    {
                        m_valuablesByType.RemoveWhere(t, c => c.ValuablesCollection.Id == lvc.Id);
                    }
                }

                if (lvc.StoryTags != null)
                {
                    foreach (var t in lvc.StoryTags)
                    {
                        m_valuablesByTag.RemoveWhere(t, c => c.ValuablesCollection.Id == lvc.Id);
                    }
                }
            }

            // Check if the collections are being updated--we don't want to
            // push through too many changes if we can help it.
            bool isUpdating;

            lock (this)
            {
                isUpdating = m_updating;

                if (isUpdating)
                {
                    m_removedValuables.Add(context.InstanceId, new LocationValuablesCollectionItemContainer(context, lvc));
                    m_addedValuables.Remove(context.InstanceId);

                    if (removedLocations != null)
                    {
                        foreach (var loc in removedLocations)
                        {
                            m_removedLocations[loc.Id] = loc;
                            m_addedLocations.Remove(loc.Id);
                        }
                    }
                }
            }

            if (!isUpdating)
            {
                if (ValuablesRemoved != null)
                {
                    ValuablesRemoved(this, new LocationValuablesCollectionManagerEventArgs(context, lvc, resetState));
                }

                if (removedLocations != null && LocationsRemoved != null)
                {
                    LocationsRemoved(this, new LocationValuablesCollectionManagerEventArgs(removedLocations.ToArray()));
                }
            }
        }

        public void AllItemsCollected(string instanceId, LocationValuablesCollection lvc)
        {
            // If this is respawnable, don't close it.
            if (lvc.CollectOptions != null &&
                lvc.CollectOptions.IsRespawnable.GetValueOrDefault())
            {
                return;
            }

            ResourceActivationContext context;

            if (m_contexts.TryGetValue(instanceId, out context))
            {
                context.Close();

                RemoveLocationValuablesCollection(context, (LocationValuablesCollection)context.Resource);
            }
        }

        public IEnumerable<LocationValuablesInstance> GetValuablesForLocation(Location location)
        {
            // First look for any valuables specifically assigned to this location
            var vals = m_valuablesByLocation[location.Id];

            lock (m_valuablesByLocation)
            {
                List<LocationValuablesInstance> vcs = null;

                if (vals != null)
                {
                    vcs = vals.ToList();
                }

                if (location.StoryTags != null)
                {
                    foreach (var tag in location.StoryTags)
                    {
                        var tagVals = m_valuablesByTag[tag];

                        if (tagVals != null)
                        {
                            if (vcs == null)
                            {
                                vcs = new List<LocationValuablesInstance>();
                            }

                            vcs.AddRange(tagVals);
                        }
                    }
                }

                if (location.LocationTypes != null)
                {
                    foreach (var type in location.LocationTypes)
                    {
                        var typeVals = m_valuablesByType[type];

                        if (typeVals != null)
                        {
                            if (vcs == null)
                            {
                                vcs = new List<LocationValuablesInstance>();
                            }

                            vcs.AddRange(typeVals);
                        }
                    }
                }

                return vcs;
            }
        }

        public void BeginUpdate()
        {
            lock (this)
            {
                m_updating = true;
            }
        }

        public void EndUpdate()
        {
            Location[] addedLocations = null;
            Location[] removedLocations = null;
            LocationValuablesCollectionItemContainer[] addedLvcs = null;
            LocationValuablesCollectionItemContainer[] removedLvcs = null;

            lock (this)
            {
                m_updating = false;

                if (m_addedLocations.Count > 0)
                {
                    addedLocations = m_addedLocations.Values.ToArray();
                    m_addedLocations.Clear();
                }

                if (m_removedLocations.Count > 0)
                {
                    removedLocations = m_removedLocations.Values.ToArray();
                    m_removedLocations.Clear();
                }

                if (m_addedValuables.Count > 0)
                {
                    addedLvcs = m_addedValuables.Values.ToArray();
                    m_addedValuables.Clear();
                }

                if (m_removedValuables.Count > 0)
                {
                    removedLvcs = m_removedValuables.Values.ToArray();
                    m_removedValuables.Clear();
                }
            }

            if (removedLocations != null && LocationsRemoved != null)
            {
                LocationsRemoved(this, new LocationValuablesCollectionManagerEventArgs(removedLocations));
            }

            if (removedLvcs != null && ValuablesRemoved != null)
            {
                ValuablesRemoved(this, new LocationValuablesCollectionManagerEventArgs(removedLvcs));
            }

            if (addedLocations != null && LocationsAdded != null)
            {
                LocationsAdded(this, new LocationValuablesCollectionManagerEventArgs(addedLocations));
            }

            if (addedLvcs != null && ValuablesAdded != null)
            {
                ValuablesAdded(this, new LocationValuablesCollectionManagerEventArgs(addedLvcs));
            }
        }
    }
}