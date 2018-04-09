// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.AR.Models;
using Motive.Core.Models;
using Motive.Core.Scripting;
using Motive.Unity.AR;
using Motive.Unity.Globalization;
using Motive.Unity.Maps;
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Task driver for AR tasks.
    /// </summary>
    public class ARTaskDriver : ARTaskDriverBase<ARTask>,
        ILocationRangeTaskDriver
    {
        //LocationFence m_fence;
        HashSet<ARWorldObject> m_worldObjects;
        ARGuideData m_guideData;

        /// <summary>
        /// Shortcut that returns simple action range. > 0 for normal range, < 0 if inverted range
        /// </summary>
        public virtual double Range
        {
            get
            {
                if (Task.ActionRange != null)
                {
                    if (Task.ActionRange.Max.HasValue)
                    {
                        return Task.ActionRange.Max.Value;
                    }
                    else
                    {
                        return -Task.ActionRange.Min.Value;
                    }
                }

                return 0;
            }
        }

        public virtual DoubleRange ActionRange
        {
            get
            {
                return Task.ActionRange;
            }
        }

        public IEnumerable<ARWorldObject> WorldObjects
        {
            get
            {
                return m_worldObjects;
            }
        }

        public ARTaskDriver(ResourceActivationContext context, ARTask task)
            : base(context, task)
        {
            m_worldObjects = new HashSet<ARWorldObject>();
        }

        public override ARWorldObject GetNearestWorldObject()
        {
            ARWorldObject worldObj = null;

            if (ForegroundPositionService.Instance.HasLocationData)
            {
                var coords = ForegroundPositionService.Instance.Position;

                worldObj = m_worldObjects
                    .OfType<LocationARWorldObject>()
                    .OrderBy(o => coords.GetDistanceFrom(o.Location.Coordinates))
                    .FirstOrDefault();
            }

            if (worldObj == null)
            {
                worldObj = m_worldObjects.FirstOrDefault();
            }

            return worldObj;
        }

        public ARWorldObject GetWorldObjectAt(Location location)
        {
            return m_worldObjects.OfType<LocationARWorldObject>().Where(o => o.Location.Id == location.Id).FirstOrDefault();
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
                    Range = Task.ActionRange,
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

        void AddTapAnnotation(ARWorldObject worldObj)
        {
            if (!Task.IsHidden)
            {
                ARAnnotationViewController.Instance.AddTapAnnotation(worldObj);

                worldObj.Clicked += worldObj_Selected;
            }
        }

        void RemoveTapAnnotation(ARWorldObject worldObj)
        {
            ARAnnotationViewController.Instance.RemoveTapAnnotation(worldObj);

            worldObj.Clicked -= worldObj_Selected;
        }

        void AddCollectibleToObject(ARWorldObject worldObj)
        {
            var collectible = FirstCollectible;

            if (!Task.IsHidden)
            {
                var text = IsTakeTask ?
                    Localize.GetLocalizedString("ARAnnotation.TapToTake", "Tap to Collect") :
                    (IsGiveTask ? Localize.GetLocalizedString("ARAnnotation.TapToPut", "Tap to Put") : null);

                ARAnnotationViewController.Instance.AddTapAnnotation(worldObj, text);

                if (collectible != null)
                {
                    ARAnnotationViewController.Instance.AddCollectibleAnnotation(worldObj, collectible, () =>
                    {
                        CollectItemFromObject(worldObj);
                    });
                }
            }

            worldObj.Clicked += worldObj_Selected;
        }

        void CollectItemFromObject(ARWorldObject obj)
        {
            obj.Selected -= worldObj_Selected;

            RemoveCollectiblesFromObject(obj);

            ARAnnotationViewController.Instance.RemoveTapAnnotation(obj);

            Action();
        }

        void worldObj_Selected(object sender, EventArgs e)
        {
            CollectItemFromObject((ARWorldObject)sender);
        }

        void RemoveCollectiblesFromObject(ARWorldObject worldObj)
        {
            ARAnnotationViewController.Instance.RemoveTapAnnotation(worldObj);

            var collectible = FirstCollectible;

            worldObj.Selected -= worldObj_Selected;

            if (collectible != null)
            {
                ARAnnotationViewController.Instance.RemoveCollectibleAnnotation(worldObj, collectible);
            }
        }

        protected override bool CheckAllTargetsVisible()
        {
            return !m_worldObjects.Any(o => !o.IsVisible);
        }

        protected override bool CheckAnyTargetsVisible()
        {
            return m_worldObjects.Any(o => o.IsVisible);
        }

        int evtCt;

        void ResetWorldObjects()
        {
            RemovePutCollectible();

            if (ARTaskAnnotationHandler.Instance)
            {
                ARTaskAnnotationHandler.Instance.RemoveTaskAnnotations(this);
            }

            foreach (var obj in m_worldObjects)
            {
                evtCt--;

                obj.EnteredView -= ObjectEnteredView;
                obj.ExitedView -= ObjectExitedView;
                obj.Selected -= worldObj_Selected;

                RemoveCollectiblesFromObject(obj);
                RemoveTapAnnotation(obj);

                ARWorld.Instance.DiscardFence(obj, ActivationContext.InstanceId);
            }

            m_worldObjects.Clear();
        }

        void SyncWorldObjects()
        {
            ResetWorldObjects();

            if (Task.ARObjectReferences != null)
            {
                foreach (var objRef in Task.ARObjectReferences)
                {
                    // Todo: if this is taken from a variable, we need to compute
                    // the instanceId from *that* script
                    var objInstanceId = ActivationContext.GetInstanceId(objRef.ObjectId);

                    var worldObjs = ARWorld.Instance.GetWorldObjects(objInstanceId);

                    if (worldObjs != null)
                    {
                        foreach (var wo in worldObjs)
                        {
                            m_worldObjects.Add(wo);

                            var locobj = wo as LocationARWorldObject;

                            if (locobj != null && locobj.Location != null)
                            {
                                if (ARTaskAnnotationHandler.Instance)
                                {
                                    ARTaskAnnotationHandler.Instance.AddTaskAnnotation(this, locobj.Location);
                                }
                            }

                            if (Task.ActionRange != null)
                            {
                                ARWorld.Instance.CreateFence(ActivationContext.InstanceId,
                                    wo,
                                    Task.ActionRange,
                                    () =>
                                    {
                                        if (IsTakeTask)
                                        {
                                        // Add collectibles to this target
                                        AddCollectibleToObject(wo);
                                        }
                                        else if (IsConfirmTask)
                                        {
                                            AddTapAnnotation(wo);
                                        }
                                        else
                                        {
                                            evtCt++;

                                            wo.EnteredView += ObjectEnteredView;
                                            wo.ExitedView += ObjectExitedView;

                                            if (wo.IsVisible)
                                            {
                                            // add interactible coll
                                            AddPutCollectible();
                                            }
                                        }
                                    },
                                    () =>
                                    {
                                        if (IsTakeTask)
                                        {
                                        // Remove collectibles from this target
                                        RemoveCollectiblesFromObject(wo);
                                        }
                                        else
                                        {
                                            RemovePutCollectible();

                                            evtCt--;

                                            wo.EnteredView -= ObjectEnteredView;
                                            wo.ExitedView -= ObjectExitedView;
                                        }
                                    });
                            }
                            else
                            {
                                if (IsTakeTask)
                                {
                                    // Add collectibles to this target
                                    AddCollectibleToObject(wo);
                                }
                                else if (IsConfirmTask)
                                {
                                    AddTapAnnotation(wo);
                                }
                                else
                                {
                                    evtCt++;

                                    wo.EnteredView += ObjectEnteredView;
                                    wo.ExitedView += ObjectExitedView;

                                    if (wo.IsVisible)
                                    {
                                        // add interactible coll
                                        AddPutCollectible();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            OnUpdated();
        }

        private void ObjectExitedView(object sender, EventArgs e)
        {
            // TODO: only if none are visible
            RemovePutCollectible();
        }

        private void ObjectEnteredView(object sender, EventArgs e)
        {
            AddPutCollectible();
        }

        void ARWorldUpdated()
        {
            ThreadHelper.Instance.CallExclusive(SyncWorldObjects);
        }

        public override void Start()
        {
            ARWorld.Instance.OnUpdated.AddListener(ARWorldUpdated);

            ThreadHelper.Instance.CallExclusive(SyncWorldObjects);

            base.Start();
        }

        public override void Stop()
        {
            ResetWorldObjects();

            ARWorld.Instance.OnUpdated.RemoveListener(ARWorldUpdated);

            base.Stop();
        }
    }

}