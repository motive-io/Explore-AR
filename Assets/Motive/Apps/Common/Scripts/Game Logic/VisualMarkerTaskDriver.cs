using Motive.AR.Models;
using Motive.AR.Vuforia;
using Motive.Core.Scripting;
using Motive.UI;
using Motive.Unity.AR;
using Motive.Unity.Globalization;
using Motive.Unity.UI;
using System;
using System.Collections.Generic;
using System.Linq;

#if MOTIVE_VUFORIA
namespace Motive.Unity.Gaming
{
    public class VisualMarkerTaskDriver : ARTaskDriverBase<VisualMarkerTask>
    {
        HashSet<ARWorldObject> m_worldObjects;
        ARGuideData m_guideData;

        public VisualMarkerTaskDriver(ResourceActivationContext context, VisualMarkerTask task)
            : base(context, task)
        {
            m_worldObjects = new HashSet<ARWorldObject>();
        }

        public override ARWorldObject GetNearestWorldObject()
        {
            return m_worldObjects.FirstOrDefault();
        }

        protected override bool CheckAllTargetsVisible()
        {
            bool allTracking = true;

            if (Task.VisualMarkers != null)
            {
                foreach (var marker in Task.VisualMarkers)
                {
                    allTracking &= VuforiaWorld.Instance.IsTracking((IVuforiaMarker)marker);
                }
            }

            return allTracking;
        }

        protected override bool CheckAnyTargetsVisible()
        {
            if (Task.VisualMarkers != null)
            {
                foreach (var marker in Task.VisualMarkers)
                {
                    if (VuforiaWorld.Instance.IsTracking((IVuforiaMarker)marker))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override void SetFocus(bool focus)
        {
            var obj = GetNearestWorldObject();

            // Todo: should use same delegate as ar catcher
            if (focus)
            {
                m_guideData = new ARGuideData
                {
                    Instructions = Task.Title,
                    WorldObject = obj
                };

                ARViewManager.Instance.SetGuide(m_guideData);
            }
            else
            {
                if (m_guideData != null)
                {
                    ARViewManager.Instance.ClearGuide(m_guideData);
                }
            }

            base.SetFocus(focus);
        }

        public override void DidComplete()
        {
            ARViewManager.Instance.SetTaskComplete(this);

            base.DidComplete();
        }

        protected override void HideTask()
        {
            VuforiaWorld.Instance.Activated.RemoveListener(UpdateState);

            VuforiaWorld.Instance.TrackingConditionMonitor.Updated -= WorldUpdatedHandler;

            if (IsTakeTask)
            {
                if (!Task.IsHidden)
                {
                    foreach (var obj in m_worldObjects)
                    {
                        ARAnnotationViewController.Instance.RemoveTapAnnotation(obj);
                    }

                    m_worldObjects.Clear();

                    VuforiaWorld.Instance.RemoveResourceObjects(ActivationContext.InstanceId);
                }
            }
            else
            {
                ARInteractiveCollectiblesManager.Instance.RemoveInteractiveCollectibles(ActivationContext.InstanceId);
            }
        }

        protected override void ShowTask()
        {
            if (IsTakeTask)
            {
                if (!Task.IsHidden)
                {
                    var collectible = FirstCollectible;

                    if (collectible != null && Task.VisualMarkers != null)
                    {
                        Action onSelect = () =>
                        {
                            Action();
                        };

                        // Let the image float on top of the page
                        var imageLayout = new Layout()
                        {
                            Position = new Motive.Core.Models.Vector(0, 0, 0.2)
                        };

                        foreach (var m in Task.VisualMarkers)
                        {
                            if (collectible.AssetInstance != null)
                            {
                                VuforiaWorld.Instance.Add3dAsset(ActivationContext, (IVuforiaMarker)m, ActivationContext.InstanceId, collectible.AssetInstance, collectible.ImageUrl, imageLayout, null, onSelect);
                            }
                            else
                            {
                                VuforiaWorld.Instance.AddMarkerImage(ActivationContext, (IVuforiaMarker)m, ActivationContext.InstanceId, collectible.ImageUrl, imageLayout, null, onSelect);
                            }
                        }
                    }
                }
            }

            VuforiaWorld.Instance.TrackingConditionMonitor.Updated += WorldUpdatedHandler;

            VuforiaWorld.Instance.Activated.AddListener(UpdateState);

            UpdateState();

            base.ShowTask();
        }

        public bool CheckAnyMarkersTracking()
        {
            if (Task.VisualMarkers != null)
            {
                foreach (var marker in Task.VisualMarkers)
                {
                    if (VuforiaWorld.Instance.IsTracking((IVuforiaMarker)marker))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void UpdateState()
        {
            if (IsTakeTask)
            {
                var worldObjects = ARWorld.Instance.GetWorldObjects(ActivationContext.InstanceId);
                var newObjs = new HashSet<ARWorldObject>();

                if (worldObjects != null)
                {
                    foreach (var obj in worldObjects)
                    {
                        if (m_worldObjects.Contains(obj))
                        {
                            m_worldObjects.Remove(obj);
                        }
                        else
                        {
                            if (!Task.IsHidden)
                            {
                                var text = Localize.GetLocalizedString("ARAnnotation.TapToTake", "Tap to Collect");

                                ARAnnotationViewController.Instance.AddTapAnnotation(obj, text);
                            }
                        }

                        newObjs.Add(obj);
                    }
                }

                foreach (var obj in m_worldObjects)
                {
                    ARAnnotationViewController.Instance.RemoveTapAnnotation(obj);
                }

                m_worldObjects = newObjs;
            }

            base.UpdateState();
        }

        private void WorldUpdated()
        {
            UpdateState();
        }

        private void WorldUpdatedHandler(object sender, EventArgs e)
        {
            UpdateState();
        }
    }

}
#endif