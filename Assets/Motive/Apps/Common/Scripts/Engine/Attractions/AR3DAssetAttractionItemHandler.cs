// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Attractions.Models;
using Motive.Core.Scripting;
using Motive.Unity.AR;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Apps.Attractions
{
    public class AR3DAssetAttractionItemHandler
: AttractionItemHandler<LocationAttractionInteractible, LocationAugmented3DAsset>
    {
        public AR3DAssetAttractionItemHandler(
            ResourceActivationContext ctxt, LocationAttractionInteractible resource, LocationAugmented3DAsset item)
            : base(ctxt, resource, item)
        {
            ctxt.FiredEvent += ctxt_FiredEvent;
        }

        void ctxt_FiredEvent(object sender, EventFiredArgs e)
        {
            if (e.EventName == "gaze")
            {
                ActivationContext.FireEvent("complete");
            }
        }

        public override void Activate(LocationAttraction attraction, bool autoplay)
        {
            ThreadHelper.Instance.CallOnMainThread(() =>
                {
                // Use attraction locations unless the item has its own
                var locations = Item.Locations ?? attraction.Locations;

                    if (locations != null)
                    {
                        ARWorld.Instance.AddLocationAugmented3dAsset(ActivationContext, Item, locations);
                    }
                });
        }

        public override void Deactivate(LocationAttraction attraction)
        {
            // We're not removing per attraction. This could lead to a bug
            // where you activate this item for multiple attractions, then
            // deactivate one attraction. The rest of the items would
            // disappear as well. We'd need to start tracking specific
            // instanceIds for each handler/attraction, but I don't think
            // it's worthwhile at this stage.
            ThreadHelper.Instance.CallOnMainThread(() =>
            {
                ARWorld.Instance.RemoveLocationAugmented3dAsset(ActivationContext.InstanceId);
            });
        }

        public override void DeactivateAll()
        {
            // The same as Deactivate, for the reason described above.
            ThreadHelper.Instance.CallOnMainThread(() =>
            {
                ARWorld.Instance.RemoveLocationAugmented3dAsset(ActivationContext.InstanceId);
            });
        }
    }

}