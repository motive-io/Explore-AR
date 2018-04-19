using System.Collections;
using System.Collections.Generic;
using Motive.AR.Media;
using Motive.Unity.Gaming;
using UnityEngine;

namespace Motive.Unity.Apps
{
    public class LocativeAudioInitializer : Initializer
    {
        protected override void Initialize()
        {
            LocativeAudioDriver.Instance.Start();
        }
    }
}
