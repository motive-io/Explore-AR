// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;

using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive.Unity.AR
{
    public class ARMarkerTrackingConditionMonitor : SynchronousConditionMonitor<VisualMarkerTrackingCondition>
    {
        Logger m_logger;

        public ARMarkerTrackingConditionMonitor() : base("motive.ar.visualMarkerTrackingCondition")
        {
            m_logger = new Logger(this);
        }

        public event EventHandler Updated;

        public void TrackingMarkersUpdated()
        {
            CheckWaitingConditions();

            if (Updated != null)
            {
                Updated(this, EventArgs.Empty);
            }
        }

        public override bool CheckState(FrameOperationContext fop, VisualMarkerTrackingCondition condition, out object[] results)
        {
            bool met = true;

            List<IVisualMarker> matching = null;

            if (condition.VisualMarkers != null)
            {
                foreach (var vm in condition.VisualMarkers.OfType<IVisualMarker>())
                {
                    var isTracking = false;
                    
                    if (vm != null)
                    {
                        m_logger.Debug("Checking tracking for marker {0}", vm.GetIdentifier());

                        isTracking = ARMarkerManager.Instance.IsTracking(vm);

                        if (isTracking)
                        {
                            if (matching == null)
                            {
                                matching = new List<IVisualMarker>();
                            }

                            matching.Add(vm);
                        }
                    }

                    met &= isTracking;
                }
            }

            if (matching != null)
            {
                results = matching.ToArray();
            }
            else
            {
                results = null;
            }

            return met;
        }
    }
}