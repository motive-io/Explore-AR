// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.AR.Models;
using Motive.Core.Models;
using Motive.Gaming.Models;
using Motive.Unity.Maps;
using Motive.Unity.UI;
using UnityEngine;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Location valuables collection driver for collecting items on the map.
    /// </summary>
    public class MapLocationCollectionDriver : SimpleMapAnnotationHandler, ILocationCollectionDriver
    {
        public LocationValuablesCollectionPanel ValuablesCollectionPanel;

        public bool AutoCollectInRange;
        public int AutoCollectDistance;

        public AudioClip CollectSound;
        public AudioSource AudioSource;

        LocationTriggerPool m_triggerPool;

        protected override void Awake()
        {
            base.Awake();

            WorldValuablesManager.Instance.RegisterCollectionDriver("motive.ar.mapLocationCollectionMechanic", this, isDefault: true);
        }

        public override void Initialize()
        {
            base.Initialize();

            m_triggerPool = new LocationTriggerPool(0, AutoCollectDistance);

            if (CollectSound && !AudioSource)
            {
                AudioSource = gameObject.AddComponent<AudioSource>();
            }

            if (AnnotationsAreShowing)
            {
                StartCollecting();
            }
        }

        public override Motive.UI.Framework.Panel GetSelectedLocationPanel()
        {
            return ValuablesCollectionPanel;
        }

        public void RemoveAnnotation(LocationSpawnItemDriverEventArgs<LocationValuablesCollection> e)
        {
            RemoveAnnotation(e.Results.SourceLocation.Id);
        }

        /// <summary>
        /// Currently only used by AR collector, to make sure we use the same code path to show the
        /// location. Ultimately the goal here is to make sure we don't double-up the annotations
        /// for this location.
        /// </summary>
        /// <param name="e"></param>
        public void AddARCollectAnnotation(LocationSpawnItemDriverEventArgs<LocationValuablesCollection> e)
        {
            MapAnnotation ann = new MapAnnotation(e.Results.SourceLocation);

            var options = e.Results.SpawnItem.CollectOptions;
            DoubleRange actionRange = new DoubleRange(0, AutoCollectDistance);

            if (options != null && options.CollectRange != null)
            {
                actionRange = options.CollectRange;
            }

            ann.Delegate = new MapAnnotationDelegate
            {
                OnSelect = (_ann) =>
                {
                    if (ValuablesCollectionPanel)
                    {
                        ValuablesCollectionPanel.ValuablesCollection = e.Results.SpawnItem.ValuablesCollection;

                        SelectedLocationPanelHandler.Instance.ShowSelectedLocationPanel(ValuablesCollectionPanel, _ann);

                        ValuablesCollectionPanel.ShowARCollect(actionRange);
                    }
                },
                OnDeselect = (_ann) =>
                    {
                        base.DeselectAnnotation(_ann);
                    },
                OnGetObjectForAnnotation = (_ann) =>
                {
                    return CreateAnnotationObject(e.Results.SpawnItem);
                }
            };

            AddAnnotation(e.Results.SourceLocation.Id, ann);
        }

        public AnnotationGameObject CreateAnnotationObject(LocationValuablesCollection lvc)
        {
            if (AnnotationPrefab)
            {
                var obj = Instantiate(AnnotationPrefab);

                var customImageAnn = obj as CustomImageAnnotation;

                if (customImageAnn)
                {
                    var collectible = ValuablesCollection.GetFirstCollectible(lvc.ValuablesCollection);

                    if (collectible != null)
                    {
                        customImageAnn.LoadImage(collectible.ImageUrl);
                    }
                }

                return obj;
            }

            return null;
        }

        public void StartCollecting()
        {
            m_triggerPool.Start();
        }

        public void StopCollecting()
        {
            m_triggerPool.Stop();
        }

        protected virtual void PlayCollectSound(MapLocationCollectionMechanic m)
        {
            if (m != null && m.SoundUrl != null)
            {
                Platform.Instance.PlaySound(m.SoundUrl);
            }
            else if (CollectSound)
            {
                AudioSource.PlayOneShot(CollectSound);
            }
        }

        string GetTriggerRequestId(LocationSpawnItemDriverEventArgs<LocationValuablesCollection> e)
        {
            return string.Format("{0}.{1}", e.Results.InstanceId, e.Results.SourceLocation.Id);
        }

        public void SpawnItem(LocationSpawnItemDriverEventArgs<LocationValuablesCollection> e, ILocationCollectionMechanic mechanic)
        {
            MapAnnotation ann = new MapAnnotation(e.Results.SourceLocation);

            var mapMechanic = mechanic as MapLocationCollectionMechanic;

            var options = e.Results.SpawnItem.CollectOptions;
            var action = mapMechanic == null ? null : mapMechanic.CollectAction;

            DoubleRange actionRange = new DoubleRange(0, AutoCollectDistance);

            if (options != null && options.CollectRange != null)
            {
                actionRange = options.CollectRange;
            }

            var actionType = (action == null) ? null : action.Type;

            bool collectWithPanel = false;

            // if no actionType is specified, defer to the AutoCollect setting
            if ((actionType == null && AutoCollectInRange)
                || (actionType != null && actionType == "motive.ar.inRangeLocationCollectionAction"))
            {
                m_triggerPool.WatchLocation(GetTriggerRequestId(e), e.Results.SourceLocation, () =>
                {
                    PlayCollectSound(mapMechanic);

                    WorldValuablesManager.Instance.Collect(e);
                }, actionRange);
            }
            else
            {
                collectWithPanel = true;
            }

            ann.Delegate = new MapAnnotationDelegate
            {
                OnSelect = (_ann) =>
                {
                    if (ValuablesCollectionPanel)
                    {
                        ValuablesCollectionPanel.ValuablesCollection = e.Results.SpawnItem.ValuablesCollection;
                        ValuablesCollectionPanel.OnAction = () =>
                        {
                            RewardManager.Instance.ShowRewards(e.Results.SpawnItem.ValuablesCollection);

                            PlayCollectSound(mapMechanic);

                            WorldValuablesManager.Instance.Collect(e);
                        };

                        SelectedLocationPanelHandler.Instance.ShowSelectedLocationPanel(ValuablesCollectionPanel, _ann);

                        //if (collectWithPanel)
                        {
                            ValuablesCollectionPanel.ShowMapCollect(actionRange);
                        }
                    }
                },
                OnDeselect = (_ann) =>
                    {
                        base.DeselectAnnotation(_ann);
                    },
                OnGetObjectForAnnotation = (_ann) =>
                {
                    return CreateAnnotationObject(e.Results.SpawnItem);
                }
            };

            AddAnnotation(e.Results.SourceLocation.Id, ann);
        }

        public void RemoveItem(LocationSpawnItemDriverEventArgs<LocationValuablesCollection> e)
        {
            m_triggerPool.StopWatching(GetTriggerRequestId(e));

            RemoveAnnotation(e.Results.SourceLocation.Id);
        }
    }

}