// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Unity.AR;
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using System;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Base class for AR task drivers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ARTaskDriverBase<T> : PlayerTaskDriver<T>
        where T : PlayerTask
    {
        /// <summary>
        /// Returns true if the AR task is tracking any AR items.
        /// </summary>
        public bool IsTrackingAnyMarkers { get; private set; }

        public ARTaskDriverBase(ResourceActivationContext context, T task)
            : base(context, task)
        {

        }

        public virtual bool TargetsAreVisible { get; protected set; }

        protected abstract bool CheckAllTargetsVisible();
        protected abstract bool CheckAnyTargetsVisible();

        protected virtual void ShowTask()
        {
            ARWorld.Instance.OnUpdated.AddListener(UpdateState);
            Inventory.Instance.Updated += Handle_Updated;

            UpdateState();
        }

        private void Handle_Updated(object sender, EventArgs e)
        {
            ThreadHelper.Instance.CallExclusive(UpdateState);
        }

        public abstract ARWorldObject GetNearestWorldObject();

        protected virtual void HideTask()
        {
            ARWorld.Instance.OnUpdated.RemoveListener(UpdateState);
            Inventory.Instance.Updated -= Handle_Updated;

            if (IsTakeTask)
            {
                //RemoveCollectiblesFromTargets();
            }
            else
            {
                ARInteractiveCollectiblesManager.Instance.RemoveInteractiveCollectibles(ActivationContext.InstanceId);
            }
        }

        public override void Start()
        {
            if (!ActivationContext.IsOpen)
            {
                return;
            }

            ThreadHelper.Instance.CallOnMainThread(ShowTask);

            base.Start();
        }

        public override void Stop()
        {
            ThreadHelper.Instance.CallOnMainThread(HideTask);

            base.Stop();
        }

        protected virtual void AddPutCollectible()
        {
            var collectible = FirstCollectible;

            if (collectible != null)
            {
                ARInteractiveCollectiblesManager.Instance.AddInteractiveCollectible(ActivationContext.InstanceId, collectible, () =>
                {
                    Action();
                });
            }
        }

        bool m_didAddCollectible;

        protected virtual void CheckAndPlacePutItems()
        {
            var wasTracking = IsTrackingAnyMarkers;
            IsTrackingAnyMarkers = CheckAnyTargetsVisible();

            if (IsTrackingAnyMarkers && !m_didAddCollectible)
            {
                var collectible = FirstCollectible;

                m_didAddCollectible = false;

                if (collectible != null)
                {
                    if (Inventory.Instance.GetCount(collectible.Id) > 0)
                    {
                        m_didAddCollectible = true;

                        ARInteractiveCollectiblesManager.Instance.AddInteractiveCollectible(ActivationContext.InstanceId, collectible, () =>
                        {
                            Action();
                        });
                    }
                }
            }
            else if (!IsTrackingAnyMarkers)
            {
                m_didAddCollectible = false;

                ARInteractiveCollectiblesManager.Instance.RemoveInteractiveCollectibles(ActivationContext.InstanceId);
            }
        }

        protected virtual void RemovePutCollectible()
        {
            ARInteractiveCollectiblesManager.Instance.RemoveInteractiveCollectibles(ActivationContext.InstanceId);
        }

        protected virtual void UpdateState()
        {
            if (IsGiveTask)
            {
                CheckAndPlacePutItems();
            }
            else
            {
                if (Task.Action == "track")
                {
                    if (CheckAllTargetsVisible())
                    {
                        Action();
                    }
                }
            }
        }
    }

}