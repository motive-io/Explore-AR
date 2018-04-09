// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Gaming.Models;
using Motive.Unity.Utilities;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Selected location panel for collecting location valuables.
    /// </summary>
    public class LocationValuablesCollectionPanel : SelectedLocationPanel
    {
        public ValuablesCollection ValuablesCollection { get; set; }

        public GameObject[] MapCollectObjects;
        public GameObject[] ARCollectObjects;

        public override void Populate(Motive.Unity.Maps.MapAnnotation data)
        {
            ObjectHelper.SetObjectsActive(MapCollectObjects, false);
            ObjectHelper.SetObjectsActive(ARCollectObjects, false);

            base.Populate(data);

            if (ValuablesCollection.CollectibleCounts != null &&
                ValuablesCollection.CollectibleCounts.Length > 0)
            {
                var collectible = ValuablesCollection.CollectibleCounts[0].Collectible;

                string title = collectible.Title;

                if (data.Location.Name != null)
                {
                    title += " at " + data.Location.Name;
                }

                SetTitle(title);
            }
        }

        internal void ShowARCollect(DoubleRange actionRange)
        {
            ObjectHelper.SetObjectsActive(MapCollectObjects, false);
            ObjectHelper.SetObjectsActive(ARCollectObjects, true);

            SetButtonFence(actionRange);
        }

        public void ShowMapCollect(DoubleRange actionRange)
        {
            ObjectHelper.SetObjectsActive(MapCollectObjects, true);
            ObjectHelper.SetObjectsActive(ARCollectObjects, false);

            SetButtonFence(actionRange);
        }
    }

}