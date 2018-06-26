// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.UI.Framework;
using Motive.Unity.Maps;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Handles the "warp" feature.
    /// </summary>
    public class WarpPanel : Panel<Coordinates>
    {

        public Text CoordinatesText;

        public bool ReturnToActualLocation;

        public override void Populate(Coordinates data)
        {
            if (CoordinatesText && data != null)
            {
                CoordinatesText.text = data.ToString();
            }

            base.Populate(data);
        }

        public void Warp(MapControllerEventArgs args)
        {
            PanelManager.Instance.Push(this, args);
        }

        public void Walk()
        {
            ForegroundPositionService.Instance.MoveToAnchorPosition(Data, 0.8);

            Back();
        }

        public void WalkX10()
        {
            ForegroundPositionService.Instance.MoveToAnchorPosition(Data, 8);

            Back();
        }

        public void Go()
        {
            if (ReturnToActualLocation)
            {
                ForegroundPositionService.Instance.SetAnchorPosition(null);
            }
            else
            {
                ForegroundPositionService.Instance.SetAnchorPosition(Data);
            }

            Back();
        }
    }
}