// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Standard interface for all location collection drivers.
    /// </summary>
    public interface ILocationCollectionDriver
    {
        void Initialize();

        void StartCollecting();
        void StopCollecting();

        void SpawnItem(LocationSpawnItemDriverEventArgs<Motive.AR.Models.LocationValuablesCollection> e, ILocationCollectionMechanic collectOptions);

        void RemoveItem(LocationSpawnItemDriverEventArgs<Motive.AR.Models.LocationValuablesCollection> e);
    }
}