// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.UI.Framework;
using Motive.Unity.Maps;
using Motive.Unity.Utilities;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Panel component that displays a compass and distance to a location.
    /// </summary>
    public class WayfinderComponent : PanelComponent<Location>
    {
        public Text Distance;
        public RotateWithCompass Compass;

        public override void Populate(Location obj)
        {
            if (Compass)
            {
                Compass.SetPointAtLocation(obj);
            }

            base.Populate(obj);
        }

        void Update()
        {
            if (Data != null)
            {
                if (Distance)
                {
                    var d = ForegroundPositionService.Instance.Position.GetDistanceFrom(Data.Coordinates);

                    Distance.text = StringFormatter.FormatDistance(d);
                }
            }
        }
    }

}