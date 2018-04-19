// Copyright (c) 2018 RocketChicken Interactive Inc.
#if MOTIVE_IAP
using System;
using System.Collections.Generic;
using System.Linq;
using Motive.Core.Models;
using Motive.Core.Utilities;
using UnityEngine;
using UnityEngine.Purchasing;
using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive.Unity.Apps
{
    /// <summary>
    /// 
    /// </summary>
    public enum EpisodeSelectState
    {
        /// <summary>Unable to launch script OR episode not purchased</summary>
        Disabled,
        /// <summary>Able to Launch/Stop/Reset.</summary>
        AvailableToLaunch,
        /// <summary>Episode is Running.</summary>
        Running,
        /// <summary>Episode is finished.</summary>
        Finished
    }

    public class ItemPurchasedEventArgs : EventArgs
    {
        public string ProductIdentifier { get; set; }

        public ItemPurchasedEventArgs(string productIdentifier)
        {
            this.ProductIdentifier = productIdentifier;
        }
    }

    public class EpisodePurchaseManager : Singleton<EpisodePurchaseManager>, IStoreListener
    {
        private Logger m_Logger;

        private IStoreController m_StoreController;          // The Unity Purchasing system.
        private IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

        public Dictionary<string, ScriptDirectoryItem> PurchasableItems { get; private set; }
        public event EventHandler<ItemPurchasedEventArgs> ItemPurchased;
        /// <summary>
        /// Called when initialization step is complete. Note that "IsInitialized" may still be
        /// false if the initialization actually failed.
        /// </summary>
        public event EventHandler InitializeComplete;
        public event EventHandler InitializeFailed;
        public bool DebugMode;
        
        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            m_Logger = new Logger(this);

            PurchasableItems = new Dictionary<string, ScriptDirectoryItem>();

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            var items = ScriptRunnerManager.Instance.GetScriptRunners().Where(item => !string.IsNullOrEmpty(item.ProductIdentifier));

            foreach (var item in items)
            {
                if (PurchasableItems.ContainsKey(item.ProductIdentifier))
                {
                    m_Logger.Warning(string.Format("PurchaseManager Purchasable item ({0}) already registered.", item.ProductIdentifier));
                    continue;
                }

                m_Logger.Verbose(string.Format("PurchaseManager Purchasable item ({0}) registered.", item.ProductIdentifier));

                PurchasableItems.Add(item.ProductIdentifier, item);
                builder.AddProduct(item.ProductIdentifier, ProductType.NonConsumable);

            }

            UnityPurchasing.Initialize(this, builder);
            m_Logger.Verbose(string.Format("PurchaseManager {0} initialized.", this.GetType()));
        }
        
        public bool IsInitialized
        {
            get
            {
                // Only say we are initialized if both the Purchasing references are set.
                return m_StoreController != null && m_StoreExtensionProvider != null;
            }
        }
        
        public EpisodeSelectState GetEpisodeState(ScriptDirectoryItem episode)
        {
            // default, unless conditions determine otherwise
            var state = EpisodeSelectState.AvailableToLaunch;

            // not available if is item that user has not purchased
            if (!string.IsNullOrEmpty(episode.ProductIdentifier) && EpisodePurchaseManager.Instance.IsInitialized)
            {
                var wasPurchased = false;
                EpisodePurchaseManager.Instance.CheckProductReceipt(episode.ProductIdentifier, () =>
                {
                    // if was purchased 
                    state = EpisodeSelectState.AvailableToLaunch;
                    wasPurchased = true;
                });

                if (!wasPurchased)
                {
                    state = EpisodeSelectState.Disabled;
                }

                // don't return, state must change if currently running 
            }

            // todo what are the conditions for which an episode is DISABLED:
            if (IsDisabled(episode))
            {
                state = EpisodeSelectState.Disabled;
            }

            var isRunning = ScriptRunnerManager.Instance.IsRunning(episode);
            if (isRunning)
            {
                state = EpisodeSelectState.Running;
            }

            return state;
        }

        public bool IsDisabled(ScriptDirectoryItem episode)
        {
            // todo
            return false;
        }

        public void BuyProductID(string productId)
        {
            // If Purchasing has been initialized ...
            if (IsInitialized)
            {
                // ... look up the Product reference with the general product identifier and the Purchasing 
                // system's products collection.
                Product product = m_StoreController.products.WithID(productId);

                // If the look up found a product for this device's store and that product is ready to be sold ... 
                if (product != null && product.availableToPurchase)
                {
                    m_Logger.Debug(string.Format("PurchaseManager Purchasing product asychronously: '{0}'", product.definition.id));
                    // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                    // asynchronously.
                    m_StoreController.InitiatePurchase(product);
                }
                // Otherwise ...
                else
                {
                    // ... report the product look-up failure situation  
                    m_Logger.Debug("PurchaseManager BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            // Otherwise ...
            else
            {
                // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
                // retrying initiailization.
                m_Logger.Debug("PurchaseManager BuyProductID FAIL. Not initialized.");
            }
        }
        
        // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
        // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
        public void RestorePurchases()
        {
            // If Purchasing has not yet been set up ...
            if (!IsInitialized)
            {
                // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
                m_Logger.Debug("PurchaseManager RestorePurchases FAIL. Not initialized.");
                return;
            }

            // If we are running on an Apple device ... 
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                // ... begin restoring purchases
                m_Logger.Debug("PurchaseManager RestorePurchases for IOS");
                m_Logger.Debug("PurchaseManager RestorePurchases started ...");

                // Fetch the Apple store-specific subsystem.
                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                apple.RestoreTransactions((result) =>
                {
                    m_Logger.Debug("PurchaseManager RestorePurchases: " + (result ? "succeeded." : "failed."));
                });
            }
            // Otherwise ...
            else
            {
                // We are not running on an Apple device. No work is necessary to restore purchases.
                m_Logger.Debug("PurchaseManager RestorePurchases does not need direct call on this platform. Current = " + Application.platform + "");
            }
        }

        public void CheckProductReceipt(string productId, Action ifHasReceipt)
        {
            // If Purchasing has not yet been set up ...
            if (!IsInitialized)
            {
                // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
                m_Logger.Debug("PurchaseManager CheckProductReceipt FAIL. Not initialized.");
                return;
            }

            var item = m_StoreController.products.WithID(productId);
            if (item != null && item.hasReceipt && ifHasReceipt != null) ifHasReceipt();
        }


        //  
        // --- IStoreListener
        //

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            // Purchasing has succeeded initializing. Collect our Purchasing references.
            m_Logger.Debug("PurchaseManager OnInitialized: PASS");

            // Overall Purchasing system, configured with products for this application.
            m_StoreController = controller;
            // Store specific subsystem, for accessing device-specific store features.
            m_StoreExtensionProvider = extensions;
            
            if (InitializeComplete != null)
            {
                InitializeComplete(this, EventArgs.Empty);
            }
        }


        public void OnInitializeFailed(InitializationFailureReason error)
        {
            // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
            m_Logger.Debug("PurchaseManager OnInitializeFailed InitializationFailureReason:" + error);

            if (InitializeFailed != null)
            {
                InitializeFailed(this, EventArgs.Empty);
            }

            if (InitializeComplete != null)
            {
                InitializeComplete(this, EventArgs.Empty);
            }
        }


        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            ScriptDirectoryItem dirItem;

            if (PurchasableItems.TryGetValue(args.purchasedProduct.definition.id, out dirItem))
            {
                m_Logger.Debug(string.Format("PurchaseManager ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));

                if (ItemPurchased != null)
                {
                    ItemPurchased(this, new ItemPurchasedEventArgs(args.purchasedProduct.definition.id));
                }

            }
            else
            {
                m_Logger.Debug(string.Format("PurchaseManager ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
            }

            return PurchaseProcessingResult.Complete;
        }


        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
            // this reason with the user to guide their troubleshooting actions.
            m_Logger.Debug(string.Format("PurchaseManager OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        }
    }
}
#endif