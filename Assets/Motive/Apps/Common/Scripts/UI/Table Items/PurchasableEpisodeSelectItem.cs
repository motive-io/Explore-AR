// Copyright (c) 2018 RocketChicken Interactive Inc.
#if MOTIVE_IAP
using Motive.Unity.Apps;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    class PurchasableEpisodeSelectItem : ScriptRunnerSelectItem
    {
        /// <summary>
        /// These objects are shown/hidden based on the episode's state
        /// 
        /// If one list of objects is shown, all others are hidden. 
        /// Exception is Disabled and NotPurchased can be shown together
        /// </summary>
        private List<GameObject[]> DisplayLists;
        public GameObject[] DisplayWhenNotPurchased;
        public GameObject[] DisplayWhenDisabled;
        public GameObject[] DisplayWhenAvailToLaunch;
        public GameObject[] DisplayWhenRunning;
        public GameObject[] DisplayWhenFin;
        public GameObject[] DisplayDebug;

        protected override void Awake()
        {
            base.Awake();

            DisplayLists = new List<GameObject[]>
        {
            DisplayWhenNotPurchased,
            DisplayWhenDisabled,
            DisplayWhenAvailToLaunch,
            DisplayWhenRunning,
            DisplayWhenFin
        };
        }

        protected EpisodeSelectState EpisodeSelectState;
        public void SetState(EpisodeSelectState selectState)
        {
            this.EpisodeSelectState = selectState;
        }
        public virtual void DetermineState()
        {
            SetState(EpisodePurchaseManager.Instance.GetEpisodeState(m_scriptDirectoryItem));
        }

        public virtual void UpdateViewState()
        {
            ObjectHelper.SetObjectsActive(DisplayLists.SelectMany(l => l), false);
            //DisplayLists.ForEach(l => l.ToList().ForEach(go => go.SetActive(false))); // hide all

            var purchasable = !string.IsNullOrEmpty(m_scriptDirectoryItem.ProductIdentifier);

            switch (EpisodeSelectState)
            {
                case EpisodeSelectState.Disabled:
                    DisplayWhenDisabled.ToList().ForEach(go => go.gameObject.SetActive(true));

                    if (purchasable && EpisodePurchaseManager.Instance.IsInitialized)
                    {
                        var wasPurchased = false;
                        EpisodePurchaseManager.Instance.CheckProductReceipt(m_scriptDirectoryItem.ProductIdentifier, () =>
                        {
                            wasPurchased = true;
                        });
                        if (!wasPurchased)
                        {
                            ObjectHelper.SetObjectsActive(DisplayWhenNotPurchased, true);
                        }
                    }
                    break;
                case EpisodeSelectState.AvailableToLaunch:
                    DisplayWhenAvailToLaunch.ToList().ForEach(go => go.SetActive(true));
                    break;
                case EpisodeSelectState.Running:
                    DisplayWhenRunning.ToList().ForEach(go => go.SetActive(true));
                    break;
                case EpisodeSelectState.Finished:
                    DisplayWhenFin.ToList().ForEach(go => go.SetActive(true));
                    break;
                default:
                    break;
            }

            if (BuildSettings.IsDebug && DisplayDebug != null && DisplayDebug.Any())
            {
                ObjectHelper.SetObjectsActive(DisplayDebug, true);
            }
        }

        public override void UpdateState()
        {
            base.UpdateState();
            DetermineState();
            UpdateViewState();
        }

        public void DoPurchase()
        {
            if (m_scriptDirectoryItem != null && EpisodePurchaseManager.Instance.IsInitialized)
            {
                EpisodePurchaseManager.Instance.ItemPurchased += ItemPurchased;

                EpisodePurchaseManager.Instance.BuyProductID(m_scriptDirectoryItem.ProductIdentifier);

                EpisodePurchaseManager.Instance.ItemPurchased -= ItemPurchased;

            }
        }

        private void ItemPurchased(object sender, ItemPurchasedEventArgs itemPurchasedEventArgs)
        {
            if (itemPurchasedEventArgs.ProductIdentifier == m_scriptDirectoryItem.ProductIdentifier)
            {
                DetermineState();
                UpdateViewState();
            }
        }
    }
}
#endif