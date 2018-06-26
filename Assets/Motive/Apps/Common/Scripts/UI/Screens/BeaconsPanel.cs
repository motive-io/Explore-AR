// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using System.Linq;
using Motive.AR.Beacons;
using Motive.Unity.Utilities;
using System;
using Motive.UI.Framework;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays a list of beacons.
    /// </summary>
    /// <seealso cref="Motive.UI.Framework.TablePanel" />
    public class BeaconsPanel : TablePanel
    {

        public BeaconItem ItemPrefab;

        void ReloadTable()
        {
            Table.Clear();

            var beaconStates = BeaconService.Instance.GetVisibleBeacons().ToArray();

            // I'd love to use Linq, but I think it breaks on iOS
            // if you're comparing value types.
            Array.Sort(beaconStates, (state1, state2) =>
            {
                var diff = state1.Distance - state2.Distance;

                if (diff < 0) return -1;

                if (diff == 0) return 0;

                return 1;
            });

            foreach (var state in beaconStates)
            {
                var item = Table.AddItem(ItemPrefab);

                item.BeaconState = state;
            }
        }

        public override void Populate()
        {
            var ident = new BeaconIdentifier("23A01AF0-232A-4518-9C0E-323FB773F5EF");

            BeaconService.Instance.StartRangingBeacons(ident, "beaconpanel", (_ident, key, states) =>
                {
                    ThreadHelper.Instance.CallOnMainThread(() =>
                        {
                            ReloadTable();
                        });
                });

            ReloadTable();
        }

        public override void DidPop()
        {
            var ident = new BeaconIdentifier("23A01AF0-232A-4518-9C0E-323FB773F5EF");

            BeaconService.Instance.StopRangingBeacons(ident, "beaconpanel");

            base.DidPop();
        }
    }

}