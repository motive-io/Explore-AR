// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.Unity.Apps;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Manages a set of collectibles that the player can use in AR mode.
    /// </summary>
    public class ARInteractiveCollectiblesManager : SingletonComponent<ARInteractiveCollectiblesManager>
    {
        public ARInteractiveCollectibleItem InteractiveItem;
        public Table InteractiveItemTable;
        public bool ShowAllItems;

        public UnityEvent OnSuccess;
        public UnityEvent OnFail;

        DictionaryDictionary<string, string, Action> m_collectibleCallbacks;
        ListDictionary<string, ARInteractiveCollectibleItem> m_interactiveCollectibles;
        HashSet<string> m_currentCollectibles;

        protected override void Awake()
        {
            base.Awake();

            m_collectibleCallbacks = new DictionaryDictionary<string, string, Action>();
            m_interactiveCollectibles = new ListDictionary<string, ARInteractiveCollectibleItem>();
            m_currentCollectibles = new HashSet<string>();
        }

        protected override void Start()
        {
            base.Start();

            InteractiveItemTable.Clear();

            AppManager.Instance.OnLoadComplete(() =>
            {
                Inventory.Instance.Updated += Inventory_Updated;

                SyncInventory();
            });
        }

        void SyncInventory()
        {
            if (ShowAllItems)
            {
                InteractiveItemTable.Clear();

                foreach (var item in Inventory.Instance.AllItems)
                {
                    ARInteractiveCollectibleItem _tblItem = null;

                    _tblItem = AddCollectibleItem(item.Collectible, () =>
                    {
                        var callbacks = m_collectibleCallbacks
                            .Where(kkv => kkv.Value.ContainsKey(item.Collectible.Id))
                            .Select(kkv => kkv.Value[item.Collectible.Id]);

                        if (callbacks != null && callbacks.Count() > 0)
                        {
                            _tblItem.Success();

                            foreach (var c in callbacks.ToArray())
                            {
                                c();
                            }

                            if (OnSuccess != null)
                            {
                                OnSuccess.Invoke();
                            }
                        }
                        else
                        {
                            _tblItem.Fail();

                            if (OnFail != null)
                            {
                                OnFail.Invoke();
                            }
                        }
                    });
                }
            }
        }

        private void Inventory_Updated(object sender, EventArgs e)
        {
            SyncInventory();
        }

        ARInteractiveCollectibleItem AddCollectibleItem(Collectible collectible, Action onSelect)
        {
            m_currentCollectibles.Add(collectible.Id);

            var item = InteractiveItemTable.AddItem(InteractiveItem);

            item.Populate(collectible);

            item.OnSelected.AddListener(() =>
            {
                onSelect();
            });

            return item;
        }

        public void AddInteractiveCollectible(string id, Collectible collectible, Action onSelect)
        {
            m_collectibleCallbacks.Add(id, collectible.Id, onSelect);

            // Each collectible should only appear once
            if (m_currentCollectibles.Contains(collectible.Id))
            {
                return;
            }

            if (InteractiveItemTable && InteractiveItem)
            {
                var item = AddCollectibleItem(collectible, onSelect);

                m_interactiveCollectibles.Add(id, item);
            }
        }

        public void RemoveInteractiveCollectibles(string id)
        {
            m_collectibleCallbacks.RemoveAll(id);

            if (InteractiveItemTable && !ShowAllItems)
            {
                var items = m_interactiveCollectibles[id];

                if (items != null)
                {
                    foreach (var i in items)
                    {
                        InteractiveItemTable.RemoveItem(i);
                        m_currentCollectibles.Remove(i.Collectible.Id);
                    }
                }
            }
        }
    }
}