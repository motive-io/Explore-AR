// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.AR.Models;
using Motive.Core.Models;
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.Gaming
{
    public class LocationTreasureChestManager : Singleton<LocationTreasureChestManager>
    {
        private SetDictionary<string, WeightedValuablesCollection> m_locationTypeChests;
        private SetDictionary<string, WeightedValuablesCollection> m_storyTagChests;

        public LocationTreasureChestManager()
        {
            m_locationTypeChests = new SetDictionary<string, WeightedValuablesCollection>();
            m_storyTagChests = new SetDictionary<string, WeightedValuablesCollection>();
        }

        public void Populate(Catalog<LocationTreasureChest> catalog)
        {
            foreach (var tc in catalog)
            {
                if (tc.TreasureChests != null)
                {
                    if (tc.StoryTags != null)
                    {
                        foreach (var t in tc.StoryTags)
                        {
                            foreach (var chest in tc.TreasureChests)
                            {
                                m_storyTagChests.Add(t, chest);
                            }
                        }
                    }

                    if (tc.LocationTypes != null)
                    {
                        foreach (var t in tc.LocationTypes)
                        {
                            foreach (var chest in tc.TreasureChests)
                            {
                                m_locationTypeChests.Add(t, chest);
                            }
                        }
                    }
                }
            }
        }

        public ValuablesCollection GetValuablesForLocation(Location location)
        {
            // Choose a valuables collection based on the
            // relative weight of each one assigned to this
            // type of location.
            List<WeightedValuablesCollection> candidates = new List<WeightedValuablesCollection>();

            double totalWeight = 0;

            if (location.StoryTags != null)
            {
                foreach (var t in location.StoryTags)
                {
                    var vals = m_storyTagChests[t];

                    if (vals != null)
                    {
                        foreach (var w in vals)
                        {
                            totalWeight += w.Weight;
                            candidates.Add(w);
                        }
                    }
                }
            }

            if (location.LocationTypes != null)
            {
                foreach (var t in location.LocationTypes)
                {
                    var vals = m_locationTypeChests[t];

                    if (vals != null)
                    {
                        foreach (var w in vals)
                        {
                            totalWeight += w.Weight;
                            candidates.Add(w);
                        }
                    }

                }
            }

            // Now choose a random number between 0 & total weight
            var r = Random.Range(0f, (float)totalWeight);

            double curr = 0.0;

            foreach (var c in candidates)
            {
                curr += c.Weight;

                if (r < curr)
                {
                    return c.ValuablesCollection;
                }
            }

            return null;
        }
    }

}