// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if MOTIVE_VUFORIA

namespace Motive.AR.Vuforia
{
    public class VuforiaMarkerTrackingConditionMonitor : SynchronousConditionMonitor<VisualMarkerTrackingCondition>
    {
        public VuforiaMarkerTrackingConditionMonitor() : base("motive.ar.visualMarkerTrackingCondition")
        {
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
                foreach (var vm in condition.VisualMarkers)
                {
                    var isTracking = false;

                    var vum = vm as IVuforiaMarker;

                    if (vum != null)
                    {
                        isTracking = VuforiaWorld.Instance.IsTracking(vum);

                        if (isTracking)
                        {
                            if (matching == null)
                            {
                                matching = new List<IVisualMarker>();
                            }

                            matching.Add(vum);
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
#endif