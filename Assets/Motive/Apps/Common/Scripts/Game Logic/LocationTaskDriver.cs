// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Scripting;
using Motive.AR.LocationServices;
using Motive.Core.Diagnostics;
using System.Linq;
using Motive.Core.Timing;
using Motive.AR.Models;
using Motive.Unity.Maps;
using Motive.Gaming.Models;
using System;
using Motive.Core.Models;
using Motive.Unity.Globalization;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Default implementation of a Location Task Driver. This manages
    /// the lifecycle and game play of Location Tasks.
    /// </summary>
    public class LocationTaskDriver : PlayerTaskDriver<LocationTask, ILocationMinigameDriver>,
        ILocationRangeTaskDriver
    {
        LocationFence m_fence;
        Location[] m_taskLocations;

        public Location ActionLocation { get; private set; }

        public virtual bool ShouldHoldLocations
        {
            get
            {
                return true;
            }
        }

        public virtual bool ShouldRememberUsedLocations
        {
            get
            {
                return true;
            }
        }

        public LocationTaskDriver(ResourceActivationContext context, LocationTask task)
            : base(context, task)
        {
        }

        public virtual string OutOfRangeActionButtonText
        {
            get
            {
                if (MinigameDriver != null)
                {
                    return MinigameDriver.OutOfRangeActionButtonText;
                }

                return null;
            }
        }

        public virtual string ActionButtonText
        {
            get
            {
                if (MinigameDriver != null)
                {
                    return MinigameDriver.ActionButtonText;
                }

                if (IsTakeTask)
                {
                    return Localize.GetLocalizedString("Task.TakeItems", "Take Items");
                }
                else if (IsGiveTask)
                {
                    return Localize.GetLocalizedString("Task.PutItems", "Put Items");
                }

                return Localize.GetLocalizedString("Task.Complete", "Complete");
            }
        }

        public virtual bool ShowActionButton
        {
            get
            {
                if (MinigameDriver != null) return MinigameDriver.ShowActionButton;

                return Task.Action != TaskAction.InRange;
            }
        }

        public virtual DoubleRange ActionRange
        {
            get
            {
                return Task.ActionRange;
            }
        }

        public virtual bool ShowMapAnnotation
        {
            get
            {
                if (Task.IsHidden) return false;

                if (MinigameDriver != null) return MinigameDriver.ShowMapAnnotation;

                return true;
            }
        }

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

        protected override ILocationMinigameDriver GetDefaultMinigameDriver()
        {
            return new DefaultLocationMinigameDriver(this);
        }

        protected override ILocationMinigameDriver GetMinigameDriver(ITaskMinigame minigame)
        {
            if (minigame is ARCatcherMinigame)
            {
                return new ARCatcherMinigameDriver(this, (ARCatcherMinigame)minigame);
            }

            return base.GetMinigameDriver(minigame);
        }

        public virtual void Complete(Location location)
        {
            ActionLocation = location;

            if (Task.TaskResultLocationVariableReference != null)
            {
                ActivationContext.SetScriptVariable(Task.TaskResultLocationVariableReference.ObjectId, location);
            }

            if (ShouldRememberUsedLocations)
            {
                RecentlyUsedLocationService.Instance.StoreLocation(location);
            }

            base.Complete();
        }

        public virtual void Action(Location location)
        {
            base.Action();
        }

        public override void Start()
        {
            if (!ActivationContext.IsOpen)
            {
                return;
            }

            ShowTask();

            // Store these - the task can be updated
            m_taskLocations = Task.Locations;

            if (Task.Locations != null &&
                Task.Locations.Length > 0)
            {
                if (ShouldHoldLocations)
                {
                    foreach (var l in Task.Locations)
                    {
                        RecentlyUsedLocationService.Instance.HoldLocation(l);
                    }
                }

                if (Task.ActionRange != null)
                {
                    m_fence = LocationFence.Watch(
                        Task.Locations,
                        Task.ActionRange.Min.GetValueOrDefault(),
                        Task.ActionRange.Max.GetValueOrDefault(double.MaxValue),
                        (inRangeLocations) =>
                        {
                        // In range
                        ActivationContext.FireEvent(TaskAction.InRange);
                            ActivationContext.SetState("in_range");

                            if (Task.Action == TaskAction.InRange)
                            {
                            // That's it! Task is comlete.
                            Complete(inRangeLocations.FirstOrDefault());
                            }
                        },
                        () =>
                        {
                        // Out of range
                        ActivationContext.ClearState("in_range");
                        });
                }
            }

            base.Start();
        }

        public override void Stop()
        {
            HideTask();

            if (ShouldHoldLocations && Task.Locations != null)
            {
                foreach (var l in Task.Locations)
                {
                    RecentlyUsedLocationService.Instance.ReleaseLocation(l);
                }
            }

            if (m_fence != null)
            {
                m_fence.StopWatching();
                m_fence = null;
            }

            base.Stop();
        }

        protected virtual void HideTask()
        {
            if (LocationTaskAnnotationHandler.Instance)
            {
                LocationTaskAnnotationHandler.Instance.RemoveTaskAnnotations(this);
            }

            if (MinigameDriver != null)
            {
                MinigameDriver.Stop();
            }
        }

        protected virtual void ShowTask()
        {
            if (ShowMapAnnotation && LocationTaskAnnotationHandler.Instance)
            {
                LocationTaskAnnotationHandler.Instance.AddTaskAnnotations(this);
            }

            if (MinigameDriver != null)
            {
                MinigameDriver.Start();
            }
        }

        public override void Update(Motive.Gaming.Models.PlayerTask task)
        {
            base.Update(task);

            if (ShouldHoldLocations)
            {
                if (m_taskLocations != null)
                {
                    foreach (var l in m_taskLocations)
                    {
                        RecentlyUsedLocationService.Instance.ReleaseLocation(l);
                    }
                }
            }

            m_taskLocations = this.Task.Locations;

            if (ShouldHoldLocations)
            {
                if (m_taskLocations != null)
                {
                    foreach (var l in m_taskLocations)
                    {
                        RecentlyUsedLocationService.Instance.HoldLocation(l);
                    }
                }
            }

            if (LocationTaskAnnotationHandler.Instance && ShowMapAnnotation)
            {
                LocationTaskAnnotationHandler.Instance.UpdateTaskAnnotations(this);
            }
        }
    }
}