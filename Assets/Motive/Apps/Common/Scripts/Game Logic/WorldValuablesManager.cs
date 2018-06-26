// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.AR.Models;
using Motive.Core.Utilities;
using Motive.Unity.Utilities;
using System.Collections.Generic;
using System.Linq;
using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Handles world valuables set up by location valuables collections. This
    /// component distributes valuables to the various collection handlers based
    /// on the collection activity defined in the LVC.
    /// </summary>
    public class WorldValuablesManager : Singleton<WorldValuablesManager>
    {
        LocationValuablesSpawnItemDriver m_spawnItemDriver;

        ILocationCollectionDriver m_defaultCollectionDriver;

        Logger m_logger;

        Dictionary<string, ILocationCollectionDriver> m_drivers;

        public WorldValuablesManager()
        {
            m_drivers = new Dictionary<string, ILocationCollectionDriver>();
        }

        public void Initialize()
        {
            m_logger = new Logger(this);

            m_spawnItemDriver = new LocationValuablesSpawnItemDriver();
            m_spawnItemDriver.ItemsSpawned += m_spawnItemDriver_ItemsSpawned;
            m_spawnItemDriver.ItemsRemoved += m_spawnItemDriver_ItemsRemoved;

            foreach (var driver in m_drivers.Values)
            {
                driver.Initialize();
            }
        }

        public void RegisterCollectionDriver(string type, ILocationCollectionDriver driver, bool isDefault = false)
        {
            // First to register is the default
            if (isDefault)
            {
                m_defaultCollectionDriver = driver;
            }

            m_drivers[type] = driver;
        }

        public void StartCollecting()
        {
            m_logger.Debug("StartCollecting");

            m_spawnItemDriver.Start();
        }

        public void StopCollecting()
        {
            m_logger.Debug("StopCollecting");

            m_spawnItemDriver.Stop();
        }

        void m_spawnItemDriver_ItemsRemoved(object sender, LocationSpawnItemDriverEventArgs<LocationValuablesCollection> e)
        {
            var drivers = GetDrivers(e.Results.SpawnItem);

            if (drivers != null)
            {
                foreach (var driver in drivers)
                {
                    driver.Key.RemoveItem(e);
                }
            }
        }

        IEnumerable<KeyValuePair<ILocationCollectionDriver, ILocationCollectionMechanic>> GetDrivers(LocationValuablesCollection lvc)
        {
            if (lvc.CollectOptions != null && lvc.CollectOptions.CollectionMechanics != null)
            {
                var drivers = lvc.CollectOptions.CollectionMechanics
                    .Where(m => m_drivers.ContainsKey(m.Type))
                    .Select(m => new KeyValuePair<ILocationCollectionDriver, ILocationCollectionMechanic>(m_drivers[m.Type], m));

                if (drivers.Count() > 0)
                {
                    return drivers;
                }
            }

            if (m_defaultCollectionDriver != null)
            {
                return new KeyValuePair<ILocationCollectionDriver, ILocationCollectionMechanic>[]
                {
                new KeyValuePair<ILocationCollectionDriver, ILocationCollectionMechanic>(m_defaultCollectionDriver, null)
                };
            }

            return null;
        }

        void m_spawnItemDriver_ItemsSpawned(object sender, LocationSpawnItemDriverEventArgs<LocationValuablesCollection> e)
        {
            var drivers = GetDrivers(e.Results.SpawnItem);

            if (drivers != null)
            {
                ThreadHelper.Instance.CallOnMainThread(() =>
                {
                    foreach (var kv in drivers)
                    {
                        kv.Key.SpawnItem(e, kv.Value);
                    }
                });
            }
        }

        public void Collect(string instanceId, Location location, LocationValuablesCollection lvc)
        {
            m_spawnItemDriver.Collect(instanceId, location, lvc);
        }

        public void Collect(LocationSpawnItemDriverEventArgs<LocationValuablesCollection> e)
        {
            Collect(e.Results.InstanceId, e.Results.SourceLocation, e.Results.SpawnItem);
        }
    }

}