// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Beacons;
using Motive.AR.Models;
using Motive.Unity.Apps;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    public class BeaconItem : MonoBehaviour
    {
        public Text NameText;
        public Text IdentText;
        public Text DistanceText;

        public BeaconState BeaconState;

        Beacon m_beacon;

        // Use this for initialization
        void Start()
        {
            if (BeaconState != null)
            {
                m_beacon = BeaconDirectory.Instance.GetBeaconByIdent(BeaconState.IdentifierKey);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (BeaconState != null)
            {
                if (m_beacon != null)
                {
                    NameText.text = m_beacon.Name;
                }

                IdentText.text = string.Format("{0:X4}-{1:X4}", BeaconState.MajorNumber, BeaconState.MinorNumber);
                DistanceText.text = string.Format("{0}m", BeaconState.Distance);
            }
        }
    }

}