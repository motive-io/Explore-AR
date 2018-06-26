// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;

namespace Motive.Unity.Beacons
{
    public class UnityBeacon : MonoBehaviour
    {
        public string ProximityUUID;
        public float Distance;
        public int Major;
        public int Minor;

        public TextMesh IdentText;
        public TextMesh DistanceText;

        UnityBeaconUser m_beaconUser;

        // Use this for initialization
        void Start()
        {
            m_beaconUser = transform.parent.gameObject.GetComponentInChildren<UnityBeaconUser>();

            IdentText.text = string.Format("{0:X4}-{1:X4}", Major, Minor);
        }

        // Update is called once per frame
        void Update()
        {
            var delta = m_beaconUser.transform.position - transform.position;

            Distance = delta.magnitude;

            DistanceText.text = Distance.ToString();
        }
    }
}