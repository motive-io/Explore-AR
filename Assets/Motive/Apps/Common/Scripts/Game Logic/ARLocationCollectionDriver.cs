// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Gaming.Models;
using Motive.Unity.AR;
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Manages collecting items at GPS locations using AR.
    /// </summary>
    public class ARLocationCollectionDriver : MonoBehaviour, ILocationCollectionDriver
    {
        public MapLocationCollectionDriver MapLocationCollectionDriver;

        Dictionary<string, LocationARWorldObject> m_worldObjects;

        void Awake()
        {
            WorldValuablesManager.Instance.RegisterCollectionDriver("motive.ar.arLocationCollectionMechanic", this);

            if (!MapLocationCollectionDriver)
            {
                MapLocationCollectionDriver = GetComponent<MapLocationCollectionDriver>();
            }
        }

        public void Initialize()
        {
            m_worldObjects = new Dictionary<string, LocationARWorldObject>();
        }

        public void StartCollecting()
        {
        }

        public void StopCollecting()
        {
        }

        private ILocationAugmentedOptions GetOptions(
            LocationValuablesCollection lvc, ARLocationCollectionMechanic m)
        {
            var opts = m.AROptions ?? ARWorld.Instance.GetDefaultImageOptions();

            if (lvc.CollectOptions != null && opts.VisibleRange == null)
            {
                opts.VisibleRange = lvc.CollectOptions.CollectRange;
            }

            return opts;
        }

        public void SpawnItem(LocationSpawnItemDriverEventArgs<LocationValuablesCollection> e, ILocationCollectionMechanic m)
        {
            var arMechanic = m as ARLocationCollectionMechanic;
            bool showAnnotation = arMechanic != null && arMechanic.ShowMapAnnotation;

            var collectible = ValuablesCollection.GetFirstCollectible(e.Results.SpawnItem.ValuablesCollection);

            var options = GetOptions(e.Results.SpawnItem, m as ARLocationCollectionMechanic);

            if (collectible != null)
            {
                ThreadHelper.Instance.CallOnMainThread(() =>
                    {
                        LocationARWorldObject worldObj = null;

                        if (collectible.AssetInstance != null &&
                            collectible.AssetInstance.Asset != null)
                        {
                            worldObj = ARWorld.Instance.AddLocationARAsset(
                                e.Results.SourceLocation,
                                0,
                                collectible.AssetInstance,
                                options);
                        }

                        if (worldObj == null)
                        {
                            worldObj = ARWorld.Instance.
                                AddLocationARImage(e.Results.SourceLocation, 0, collectible.ImageUrl, options);
                        }

                        worldObj.Clicked += (sender, args) =>
                        {
                            RewardManager.Instance.ShowRewards(e.Results.SpawnItem.ValuablesCollection);

                            WorldValuablesManager.Instance.Collect(e);
                        };

                        m_worldObjects[e.Results.SourceLocation.Id] = worldObj;
                    });

                if (showAnnotation && MapLocationCollectionDriver)
                {
                    MapLocationCollectionDriver.AddARCollectAnnotation(e);
                }
            }
        }

        public void RemoveItem(LocationSpawnItemDriverEventArgs<Motive.AR.Models.LocationValuablesCollection> e)
        {
            if (m_worldObjects.ContainsKey(e.Results.SourceLocation.Id))
            {
                var obj = m_worldObjects[e.Results.SourceLocation.Id];

                ThreadHelper.Instance.CallOnMainThread(() =>
                    {
                        ARWorld.Instance.RemoveWorldObject(obj);

                        if (MapLocationCollectionDriver)
                        {
                            MapLocationCollectionDriver.RemoveAnnotation(e);
                        }
                    });
            }
        }
    }
}