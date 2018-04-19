using System.Collections;
using System.Collections.Generic;
using Motive.Unity.Gaming;
using UnityEngine;

namespace Motive.Unity.Apps
{
    public class WorldValuablesInitializer : Initializer
    {
        public bool AutoStartCollecting = true;

        protected override void Initialize()
        {
            WorldValuablesManager.Instance.Initialize();

            if (AutoStartCollecting)
            {
                WorldValuablesManager.Instance.StartCollecting();
            }
        }
    }
}
