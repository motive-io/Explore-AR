// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.AR.Models;
using Motive.Attractions.Models;
using Motive.Core.Models;
using Motive.Core.Scripting;
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.Unity.AR;
using Motive.Unity.Utilities;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Motive.Unity.Maps;

namespace Motive.Unity.Apps.Attractions
{
    public interface IAttractionItemHandler
    {
        ResourceActivationContext ActivationContext { get; }
        LocationAttractionItemProperties ItemProperties { get; }

        void Activate(LocationAttraction attraction, bool autoplay);
        void Deactivate(LocationAttraction attraction);

        void DeactivateAll();
    }

    public abstract class AttractionItemHandler<R, I>
        : IAttractionItemHandler
        where R : LocationAttractionItemProperties
        where I : IScriptObject
    {
        public R Resource { get; private set; }
        public I Item { get; private set; }
        public LocationAttractionItemProperties ItemProperties { get; private set; }

        public ResourceActivationContext ActivationContext
        {
            get;
            private set;
        }

        public AttractionItemHandler(ResourceActivationContext ctxt, R resource, I item)
        {
            this.ItemProperties = resource;
            this.ActivationContext = ctxt;
            this.Resource = resource;
            this.Item = item;
        }

        public abstract void Activate(LocationAttraction attraction, bool autoplay);

        public abstract void Deactivate(LocationAttraction attraction);

        public abstract void DeactivateAll();
    }

    /// <summary>
    /// Provides a container for an activated attraction. Tracks interactibles
    /// and content enabled for this attraction.
    /// </summary>
    public class ActivatedAttractionContext
    {
        public LocationAttraction Attraction { get; private set; }
        public DoubleRange Range { get; set; }

        Dictionary<string, ActiveResourceContainer<LocationAttractionActivator>> m_activationContexts;

        Dictionary<string, IAttractionItemHandler> m_itemHandlers;

        public IEnumerable<IAttractionItemHandler> ItemHandlers
        {
            get
            {
                return m_itemHandlers.Values;
            }
        }

        public ActivatedAttractionContext(LocationAttraction attraction)
        {
            Attraction = attraction;
            m_activationContexts = new Dictionary<string, ActiveResourceContainer<LocationAttractionActivator>>();
            m_itemHandlers = new Dictionary<string, IAttractionItemHandler>();
        }

        public bool IsUnlocked(ResourceActivationContext ctxt, LocationAttractionItemProperties props)
        {
            switch (props.RangeAvailability)
            {
                case AttractionRangeAvailability.Always:
                    return true;
                case AttractionRangeAvailability.InRangeAndAfter:
                    return ctxt.CheckEvent("in_range");
                case AttractionRangeAvailability.OnlyInRange:
                    return ctxt.CheckState("in_range");
            }

            return false;
        }

        public bool IsToDo(ResourceActivationContext ctxt, LocationAttractionItemProperties props)
        {
            return !ctxt.CheckEvent("complete");
        }

        public void AddActivator(ResourceActivationContext ctxt, LocationAttractionActivator attraction)
        {
            m_activationContexts[ctxt.InstanceId] = new ActiveResourceContainer<LocationAttractionActivator>(ctxt);

            // Shortcut: just set the range to whatever the most recent attraction has it set to.
            // We can implement a smarter policy at some point, but activating an attraction
            // multiple times doesn't have an obvious use case
            if (attraction.Range != null)
            {
                Range = attraction.Range;
            }
        }

        public void RemoveActivator(string instanceId)
        {
            m_activationContexts.Remove(instanceId);
        }

        public void AddItemHandler(IAttractionItemHandler handler)
        {
            m_itemHandlers[handler.ActivationContext.InstanceId] = handler;
        }

        public void RemoveItemHandler(IAttractionItemHandler handler)
        {
            m_itemHandlers.Remove(handler.ActivationContext.InstanceId);
        }

        public bool IsActive
        {
            get
            {
                return m_activationContexts.Count > 0;
            }
        }

        /// <summary>
        /// Returns all items marked as "todo" in activation order
        /// </summary>
        public IEnumerable<IAttractionItemHandler> ToDoItems
        {
            get
            {
                var allItems =
                    m_itemHandlers.Values.Where(i => IsToDo(i.ActivationContext, i.ItemProperties))
                    .OrderBy(i => i.ActivationContext.ActivationTime);

                return allItems;
            }
        }

        public bool HasToDos
        {
            get
            {
                return m_itemHandlers.Values.Any(i => IsToDo(i.ActivationContext, i.ItemProperties));
            }
        }
    }

    /// <summary>
    /// Manages attractions and anything attached to them.
    /// </summary>
    public class AttractionManager : SingletonSimpleMapAnnotationHandler<AttractionManager, MapAnnotation<ActivatedAttractionContext>>
    {
        public UnityEvent OnUpdated;

        // Keeps track of attractions/activator instance IDs to make sure we only
        // activate/deactivate attractions once and always keep them alive
        // as long as there's at least one activator active.
        private Dictionary<string, ActivatedAttractionContext> m_atrractionActivators;
        private SetDictionary<string, IAttractionItemHandler> m_attractionItemHandlers;
        private Dictionary<string, IAttractionItemHandler> m_itemHandlers;
        LocationFencePool m_fencePool;
        LocationTriggerPool m_triggerPool;

        public IEnumerable<ActivatedAttractionContext> ActiveAttractions
        {
            get
            {
                return m_atrractionActivators.Values;
            }
        }

        protected override void Awake()
        {
            m_atrractionActivators = new Dictionary<string, ActivatedAttractionContext>();
            // Item handlers per attraction
            m_attractionItemHandlers = new SetDictionary<string, IAttractionItemHandler>();
            m_itemHandlers = new Dictionary<string, IAttractionItemHandler>();
            m_fencePool = new LocationFencePool();
            m_triggerPool = new LocationTriggerPool();

            AppManager.Instance.Initialized += App_Initialized;

            if (OnUpdated == null)
            {
                OnUpdated = new UnityEvent();
            }

            base.Awake();
        }

        public ActivatedAttractionContext GetNextToDo()
        {
            // TODO: order by when they were activated
            foreach (var ctxt in m_atrractionActivators.Values)
            {
                if (ctxt.HasToDos)
                {
                    return ctxt;
                }
            }

            return null;
        }

        void OnUpdate()
        {
            if (OnUpdated != null)
            {
                OnUpdated.Invoke();
            }
        }

        void App_Initialized(object sender, System.EventArgs e)
        {
            m_fencePool.Start();
            m_triggerPool.Start();
        }

        private void StopMontitoringAttractionItem(ResourceActivationContext ctxt)
        {
            m_fencePool.StopWatching(ctxt.InstanceId);
        }

        /// <summary>
        /// Starts monitoring an attraction item by setting up a location fence and following
        /// the activation/range rules. Will call back "activate" and "deactivate" based
        /// on these rules.
        /// </summary>
        /// <param name="ctxt"></param>
        /// <param name="actxt"></param>
        /// <param name="item"></param>
        /// <param name="activateItems"></param>
        /// <param name="deactivateItems"></param>
        private void MonitorAttractionItem(
            ResourceActivationContext ctxt,
            ActivatedAttractionContext actxt,
            LocationAttractionItemProperties item,
            IAttractionItemHandler handler)
        {
            bool activated = false;
            var wasInRange = ctxt.CheckEvent("in_range");

            Func<bool> checkAutoplay = () =>
            {
                var didPlay = ctxt.CheckEvent("open");

                return (item.AutoplayBehavior == AttractionRangeAutoplayBehavior.Always ||
                     item.AutoplayBehavior == AttractionRangeAutoplayBehavior.Once && !didPlay);
            };

            var autoplay = checkAutoplay();

            if (item.RangeAvailability == AttractionRangeAvailability.Always)
            {
                // Always activate
                activated = true;
                handler.Activate(actxt.Attraction, autoplay);
            }
            else
            {
                if (wasInRange && item.RangeAvailability == AttractionRangeAvailability.InRangeAndAfter)
                {
                    handler.Activate(actxt.Attraction, autoplay);
                }
            }

            var range = item.Range ?? actxt.Range;

            if (range != null && actxt.Attraction.Locations != null)
            {
                m_fencePool.WatchLocations(ctxt.InstanceId, actxt.Attraction.Locations, range,
                    (locations) =>
                    {
                        ctxt.FireEvent("in_range");
                        ctxt.SetState("in_range");

                    // In Range
                    if (!activated || item.RangeAvailability == AttractionRangeAvailability.OnlyInRange)
                        {
                            activated = true;
                            handler.Activate(actxt.Attraction, checkAutoplay());
                        }
                    },
                    () =>
                    {
                        ctxt.ClearState("in_range");

                    // Out of range
                    if (item.RangeAvailability == AttractionRangeAvailability.OnlyInRange)
                        {
                            handler.Deactivate(actxt.Attraction);
                        }
                    });
            }
            else
            {
                // Items haven't been activated, but there's no range or locations.
                // In this case treat the items as "in range" and activate them.
                if (!activated)
                {
                    ctxt.FireEvent("in_range");
                    ctxt.SetState("in_range");

                    activated = true;
                    handler.Activate(actxt.Attraction, autoplay);
                }
            }
        }

        void ActivateItemHandler(IAttractionItemHandler handler)
        {
            m_itemHandlers[handler.ActivationContext.InstanceId] = handler;

            if (handler.ItemProperties.AttractionReferences != null)
            {
                foreach (var a in handler.ItemProperties.AttractionReferences)
                {
                    ActivatedAttractionContext actxt = null;

                    if (m_atrractionActivators.TryGetValue(a.ObjectId, out actxt))
                    {
                        AttachHandlerToAttraction(actxt, handler);
                    }
                }
            }
        }

        void DetachHandlerFromAttraction(ActivatedAttractionContext actxt, IAttractionItemHandler handler)
        {
            handler.Deactivate(actxt.Attraction);

            actxt.RemoveItemHandler(handler);

            StopMontitoringAttractionItem(handler.ActivationContext);
        }

        void AttachHandlerToAttraction(ActivatedAttractionContext actxt, IAttractionItemHandler handler)
        {
            actxt.AddItemHandler(handler);

            MonitorAttractionItem(handler.ActivationContext, actxt, handler.ItemProperties, handler);
        }

        public void ActivateContent(ResourceActivationContext context, LocationAttractionContent resource)
        {
            var handler = new PlayableContentAttractionItemHandler(context, resource);

            ActivateItemHandler(handler);

            OnUpdate();
        }

        public void DeactivateContent(ResourceActivationContext context, LocationAttractionContent resource)
        {
            DeactivateAttractionItem(context, resource);
        }

        public void ActivateInteractible(ResourceActivationContext ctxt, LocationAttractionInteractible interactible)
        {
            IAttractionItemHandler handler = null;

            if (interactible.InteractibleItem != null)
            {
                if (interactible.InteractibleItem is LocationAugmented3DAsset)
                {
                    handler = new AR3DAssetAttractionItemHandler(ctxt, interactible, (LocationAugmented3DAsset)interactible.InteractibleItem);
                }
                else if (interactible.InteractibleItem is LocationAugmentedImage)
                {
                    handler = new ARImageAttractionItemHandler(ctxt, interactible, (LocationAugmentedImage)interactible.InteractibleItem);
                }
            }

            if (handler != null)
            {
                ActivateItemHandler(handler);
            }

            OnUpdate();
        }

        public void DeactivateInteractible(ResourceActivationContext context, LocationAttractionInteractible interactible)
        {
            DeactivateAttractionItem(context, interactible);
        }

        void DeactivateAttractionItem(ResourceActivationContext context, LocationAttractionItemProperties item)
        {
            IAttractionItemHandler handler = null;

            if (m_itemHandlers.TryGetValue(context.InstanceId, out handler))
            {
                if (item.AttractionReferences != null)
                {
                    foreach (var a in item.AttractionReferences)
                    {
                        m_attractionItemHandlers.Remove(a.ObjectId, handler);

                        ActivatedAttractionContext actxt;

                        if (m_atrractionActivators.TryGetValue(a.ObjectId, out actxt))
                        {
                            DetachHandlerFromAttraction(actxt, handler);
                        }
                    }
                }

                // Clean up any leftovers
                handler.DeactivateAll();
            }
        }

        public void ActivateAttractions(ResourceActivationContext ctxt, LocationAttractionActivator activator)
        {
            if (activator.AttractionReferences != null)
            {
                foreach (var a in activator.AttractionReferences)
                {
                    ActivateAttraction(ctxt, activator, a.Object);
                }
            }
        }

        private void ActivateAttraction(ResourceActivationContext ctxt, LocationAttractionActivator activator, LocationAttraction attraction)
        {
            ActivatedAttractionContext atctxt = null;

            if (!m_atrractionActivators.TryGetValue(attraction.Id, out atctxt))
            {
                atctxt = new ActivatedAttractionContext(attraction);

                m_atrractionActivators.Add(attraction.Id, atctxt);

                var annotation = new MapAnnotation<ActivatedAttractionContext>(attraction.Locations.First(), atctxt);

                AddAnnotation(attraction.Id, annotation);

                var handlers = m_attractionItemHandlers[attraction.Id];

                if (handlers != null)
                {
                    foreach (var handler in handlers)
                    {
                        AttachHandlerToAttraction(atctxt, handler);
                    }
                }
            }

            atctxt.AddActivator(ctxt, activator);
        }

        private void DeactivateAttraction(ResourceActivationContext ctxt, LocationAttractionActivator activator, LocationAttraction attraction)
        {
            ActivatedAttractionContext atctxt = null;

            if (m_atrractionActivators.TryGetValue(attraction.Id, out atctxt))
            {
                atctxt.RemoveActivator(ctxt.InstanceId);

                if (!atctxt.IsActive)
                {
                    m_atrractionActivators.Remove(attraction.Id);

                    RemoveAnnotation(atctxt.Attraction.Id);
                }

                var handlers = atctxt.ItemHandlers.ToArray();

                foreach (var handler in handlers)
                {
                    DetachHandlerFromAttraction(atctxt, handler);
                }
            }
        }

        public void DeactivateAttractions(ResourceActivationContext ctxt, LocationAttractionActivator activator)
        {
            List<LocationAttraction> toDeactivate = new List<LocationAttraction>();

            lock (m_atrractionActivators)
            {
                if (activator.AttractionReferences != null)
                {
                    foreach (var a in activator.AttractionReferences)
                    {
                        DeactivateAttraction(ctxt, activator, a.Object);
                    }
                }
            }
        }
    }
}