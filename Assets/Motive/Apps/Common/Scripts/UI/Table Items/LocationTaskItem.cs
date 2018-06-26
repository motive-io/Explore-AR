// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using Motive.Unity.Maps;
using System.Linq;

namespace Motive.Unity.UI
{
    public class LocationTaskItem : TaskItem
    {
        public override void Populate(IPlayerTaskDriver driver)
        {
            var locDriver = driver as LocationTaskDriver;

            if (Subtitle)
            {
                Subtitle.text = "";

                if (locDriver.Task.Locations != null)
                {
                    var loc = locDriver.Task.Locations.FirstOrDefault();

                    if (loc != null && Subtitle)
                    {
                        Subtitle.text = loc.Name;
                    }
                }
            }

            base.Populate(driver);
        }

        public void ShowLocation()
        {
            LocationTaskAnnotationHandler.Instance.SelectTaskAnnotation(Driver);
            PanelManager.Instance.HideAll();
        }
    }

}