// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Core;
using System.Linq;

namespace Motive.Unity.Apps
{
    public class BeaconDirectory : SingletonAssetDirectory<BeaconDirectory, Beacon>
    {
        public Beacon GetBeaconByIdent(string identifierKey)
        {
            return GetItemsWhere(i => i.IdentifierKey == identifierKey).FirstOrDefault();
        }
    }

}