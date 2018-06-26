// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive._3D.Models;
using Motive.Core.Scripting;
using Motive.Core.Utilities;
using Motive.Unity.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Scripting
{
    public class WorldObjectProximityConditionMonitor : SynchronousConditionMonitor<WorldObjectProximityCondition>
    {
        static WorldObjectProximityConditionMonitor g_instance;

        public static WorldObjectProximityConditionMonitor Instance
        {
            get
            {
                if (g_instance == null)
                {
                    g_instance = new WorldObjectProximityConditionMonitor();
                }

                return g_instance;
            }
        }

        class ConditionState
        {
            public string[] InstanceIds;
            public FrameOperationContext FrameOperationContext;
            public WorldObjectProximityCondition Condition;
            public bool? IsMet;
        };

        Dictionary<string, ConditionState> m_waitingConditions;

        WorldObjectProximityConditionMonitor() : base("motive.3d.worldObjectProximityCondition")
        {
            m_waitingConditions = new Dictionary<string, ConditionState>();
        }

        public void Update()
        {
            List<ConditionState> updates = null;

            foreach (var c in m_waitingConditions.Values)
            {
                object[] results;

                var isMet = CheckCondition(c.InstanceIds, c.FrameOperationContext, c.Condition, out results);

                if (!c.IsMet.HasValue || c.IsMet.Value != isMet)
                {
                    if (updates == null)
                    {
                        updates = new List<ConditionState>();
                    }

                    updates.Add(c);
                }

                c.IsMet = isMet;
            }

            if (updates != null)
            {
                foreach (var c in updates)
                {
                    OnUpdate(c.FrameOperationContext, c.Condition, c.IsMet.Value, null);
                }
            }
        }

        public override void WaitForConditionUpdate(FrameOperationContext fop, WorldObjectProximityCondition condition)
        {
            if (condition.WorldObjectReferences == null &&
                condition.Range == null)
            {
                return;
            }

            var state = new ConditionState()
            {
                InstanceIds = condition.WorldObjectReferences.Select(r => fop.GetInstanceId(r.ObjectId)).ToArray(),
                FrameOperationContext = fop,
                Condition = condition
            };

            m_waitingConditions[fop.GetInstanceId(condition.Id)] = state;

            base.WaitForConditionUpdate(fop, condition);
        }

        public override void StopWaiting(FrameOperationContext fop, WorldObjectProximityCondition condition)
        {
            m_waitingConditions.Remove(fop.GetInstanceId(condition.Id));

            base.StopWaiting(fop, condition);
        }

        bool CheckCondition(IEnumerable<string> instanceIds, FrameOperationContext fop, WorldObjectProximityCondition condition, out object[] results)
        {
            results = null;

            if (!ARWorld.Instance.IsActive)
            {
                return false;
            }

            foreach (var instId in instanceIds)
            {
                var objs = ARWorld.Instance.GetWorldObjects(instId);

                if (objs != null)
                {
                    foreach (var obj in objs)
                    {
                        if (obj.GameObject.activeSelf)
                        {
                            var distance = ARWorld.Instance.GetDistance(obj);

                            if (condition.Range.IsInRange(distance))
                            {
                                // Todo: gather and return results
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public override bool CheckState(FrameOperationContext fop, WorldObjectProximityCondition condition, out object[] results)
        {
            results = null;

            if (condition.WorldObjectReferences == null ||
                condition.Range == null)
            {
                return false;
            }

            foreach (var objRef in condition.WorldObjectReferences)
            {
                var instIds = condition.WorldObjectReferences.Select(o => fop.GetInstanceId(o.ObjectId));

                return CheckCondition(instIds, fop, condition, out results);
            }

            return false;
        }
    }
}