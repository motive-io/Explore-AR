// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.Core.Models;
using Motive.Core.Utilities;
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Handles annotations for task map annotations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
    public class SingletonTaskAnnotationHandler<T, D>
        : SingletonSimpleMapAnnotationHandler<T, MapAnnotation<D>>
        where T : SingletonSimpleMapAnnotationHandler<T, MapAnnotation<D>>
        where D : IPlayerTaskDriver
    {
        public UnityEngine.Color RangeCircleColor;
        public UnityEngine.Color OutOfRangeColor;
        public UnityEngine.Color InvertRangeCircleColor;

        public bool UseTaskImageForAnnotation;

        private SetDictionary<string, MapAnnotation<D>> m_taskAnnotations;

        protected override void Awake()
        {
            m_taskAnnotations = new SetDictionary<string, MapAnnotation<D>>();

            base.Awake();
        }

        public virtual void RemoveTaskAnnotation(D driver, MapAnnotation<D> ann)
        {
            m_taskAnnotations.Remove(driver.ActivationContext.InstanceId, ann);

            RemoveAnnotation(ann);
        }

        public virtual void RemoveTaskAnnotation(D driver, Location location)
        {
            var annotations = m_taskAnnotations[driver.ActivationContext.InstanceId];

            if (annotations != null)
            {
                var toRemove = annotations.Where(a => a.Location.Id == location.Id).ToArray();

                foreach (var ann in toRemove)
                {
                    RemoveTaskAnnotation(driver, ann);
                }
            }
        }

        public virtual void RemoveTaskAnnotations(D driver)
        {
            var annotations = m_taskAnnotations[driver.ActivationContext.InstanceId];

            if (annotations != null)
            {
                ThreadHelper.Instance.CallOnMainThread(() =>
                {
                    foreach (var ann in annotations.ToArray())
                    {
                        RemoveAnnotation(ann);
                    }
                });
            }
        }

        public virtual void AddTaskAnnotation(D driver, MapAnnotation<D> annotation)
        {
            m_taskAnnotations.Add(driver.ActivationContext.InstanceId, annotation);

            AddAnnotation(annotation);
        }

        public virtual void AddTaskAnnotations(D driver, IEnumerable<Location> locations)
        {
            if (locations == null) return;

            foreach (var location in locations)
            {
                AddTaskAnnotation(driver, location);
            }
        }

        public virtual MapAnnotation<D> AddTaskAnnotation(D driver, Location location)
        {
            var annotation = new MapAnnotation<D>(location, driver);

            AddTaskAnnotation(driver, annotation);

            return annotation;
        }

        public MapAnnotation<D> GetNearestAnnotation(D driver)
        {
            var annotations = m_taskAnnotations[driver.ActivationContext.InstanceId];

            var pos = ForegroundPositionService.Instance.Position;

            if (annotations != null)
            {
                return annotations
                    .OrderBy(a => a.Coordinates.GetDistanceFrom(pos))
                    .FirstOrDefault();
            }

            return null;
        }

        public virtual IEnumerable<MapAnnotation<D>> GetAnnotationsForTask(D driver)
        {
            return m_taskAnnotations[driver.ActivationContext.InstanceId];
        }

        public virtual MapAnnotation<D> GetAnnotationForTask(D driver, Location location)
        {
            var anns = GetAnnotationsForTask(driver);

            if (anns != null)
            {
                return anns.Where(a => a.Location == location).FirstOrDefault();
            }

            return null;
        }

        public virtual void SelectTaskAnnotation(IPlayerTaskDriver taskDriver)
        {
            var annotations = m_taskAnnotations[taskDriver.ActivationContext.InstanceId];

            if (annotations != null)
            {
                var ann = annotations.FirstOrDefault();

                if (ann != null)
                {
                    MapController.Instance.SelectAnnotation(ann);
                }
            }
        }

        /// <summary>
        /// Updates task annotations any time the driver updates.
        /// </summary>
        /// <param name="taskDriver"></param>
        /// <param name="locations"></param>
        public virtual void UpdateTaskAnnotations(D taskDriver, IEnumerable<Location> locations)
        {
            var annotations = GetAnnotationsForTask(taskDriver);

            if (annotations != null && locations == null)
            {
                RemoveTaskAnnotations(taskDriver);
            }
            else if (annotations == null && locations != null)
            {
                AddTaskAnnotations(taskDriver, locations);
            }
            else
            {
                var curr = annotations.ToList();

                foreach (var loc in locations)
                {
                    if (!curr.Any(a => a.Location.Id == loc.Id))
                    {
                        AddTaskAnnotation(taskDriver, loc);
                    }
                }

                foreach (var ann in curr)
                {
                    if (!locations.Any(l => l.Id == ann.Location.Id))
                    {
                        RemoveTaskAnnotation(taskDriver, ann);
                    }
                }
            }
        }

        /// <summary>
        /// Configures the annotation object. In this case looks for a task info component and populates
        /// it with annotation data.
        /// </summary>
        /// <param name="obj"></param>
        protected override void ConfigureAnnotationObject(MapAnnotation<D> annotation, AnnotationGameObject obj)
        {
            var infoComponent = obj.GetComponentInChildren<PlayerTaskInfoComponent>();

            if (infoComponent)
            {
                infoComponent.Populate(annotation.GetData());
            }

            base.ConfigureAnnotationObject(annotation, obj);
        }

        protected virtual bool ShouldSetPanelFence(D driver, out DoubleRange range)
        {
            range = null;

            return false;
        }

        protected override void ConfigureSelectedLocationPanel(Panel panel, MapAnnotation<D> annotation)
        {
            var locationPanel = panel as SelectedLocationPanel;

            if (locationPanel)
            {
                DoubleRange range = null;

                if (ShouldSetPanelFence(annotation.Data, out range))
                {
                    locationPanel.SetButtonFence(range);
                }
            }

            base.ConfigureSelectedLocationPanel(panel, annotation);
        }
    }

}