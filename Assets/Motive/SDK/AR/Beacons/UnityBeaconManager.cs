// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Beacons;
using Motive.Core.Utilities;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.Beacons
{
    public class UnityBeaconManager : SingletonComponent<UnityBeaconManager>, IBeaconManager
    {
        public float UpdateInterval = 1f;

        float m_nextFireTime;

        class BeaconMonitorContext
        {
            public BeaconIdentifier BeaconIdentifier;
            public Action<BeaconIdentifier, IEnumerable<BeaconState>> OnUpdate;
        }

        Dictionary<string, BeaconMonitorContext> m_monitorBeacons;

        protected override void Start()
        {
            m_monitorBeacons = new Dictionary<string, BeaconMonitorContext>();
            m_nextFireTime = Time.time + UpdateInterval;
        }

        // Update is called once per frame
        void Update()
        {
            //Debug.Log("time=" + Time.time + "next fire time=" + m_nextFireTime);

            if (Time.time >= m_nextFireTime)
            {
                ListDictionary<string, BeaconState> proxStates = new ListDictionary<string, BeaconState>();
                ListDictionary<BeaconMonitorContext, BeaconState> callContexts = new ListDictionary<BeaconMonitorContext, BeaconState>();

                foreach (var b in GetComponentsInChildren<UnityBeacon>())
                {
                    var state = new BeaconState
                    {
                        Timestamp = DateTime.Now,
                        ProximityUUID = b.ProximityUUID,
                        Distance = b.Distance,
                        MajorNumber = b.Major,
                        MinorNumber = b.Minor
                    };

                    proxStates.Add(b.ProximityUUID, state);

                    var ident = new BeaconIdentifier(state.ProximityUUID, state.MajorNumber, state.MinorNumber);

                    if (m_monitorBeacons.ContainsKey(ident.IdentifierKey))
                    {
                        var ctxt = m_monitorBeacons[ident.IdentifierKey];

                        callContexts.Add(ctxt, state);
                    }
                }

                foreach (var ctxt in callContexts.Keys)
                {
                    ctxt.OnUpdate(ctxt.BeaconIdentifier, callContexts[ctxt]);
                }

                foreach (var proxId in proxStates.Keys)
                {
                    var ident = new BeaconIdentifier(proxId, 0, 0);

                    if (m_monitorBeacons.ContainsKey(ident.IdentifierKey))
                    {
                        m_monitorBeacons[ident.IdentifierKey].OnUpdate(ident, proxStates[proxId]);
                    }
                }

                m_nextFireTime += UpdateInterval;
            }
        }

        public void StartRanging(BeaconIdentifier beaconIdentifier, Action<BeaconIdentifier, IEnumerable<BeaconState>> onUpdate)
        {
            BeaconMonitorContext ctxt = new BeaconMonitorContext
            {
                BeaconIdentifier = beaconIdentifier,
                OnUpdate = onUpdate
            };

            m_monitorBeacons[beaconIdentifier.IdentifierKey] = ctxt;
        }

        public void StopRanging(BeaconIdentifier beaconIdentifier)
        {
            m_monitorBeacons.Remove(beaconIdentifier.IdentifierKey);
        }
    }

}