// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Utilities;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using JsonFx.Json;
using Motive;
using Motive.Core.Scripting;
using Motive.Core.Json;
using Motive.Gaming.Models;
using Motive.Core.Storage;
using Motive.Core.Timing;
using Motive.Unity.Scripting;
using Motive.Unity.Storage;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Information about a collectible in the inventory.
    /// </summary>
    public class InventoryCollectible
    {
        public Collectible Collectible { get; private set; }
        public int Count { get; private set; }
        public DateTime? CollectTime { get; private set; }

        public InventoryCollectible(Collectible collectible, int count, DateTime? collectTime = null)
        {
            this.Collectible = collectible;
            this.Count = count;
            this.CollectTime = collectTime;
        }
    }

    /// <summary>
    /// Stores all collectibles collected by the user. Results are persisted.
    /// </summary>
    public class Inventory : Singleton<Inventory>
    {
        class InventoryItemState
        {
            public string CollectibleId { get; set; }
            public int Count { get; set; }
            public int? Limit { get; set; }
            public DateTime? CollectTime { get; set; }
        }

        class InventoryState
        {
            public InventoryItemState[] Items { get; set; }
        }

        private Dictionary<string, InventoryItemState> m_inventoryItems;

        /// <summary>
        /// Fires whenever the inventory is updated.
        /// </summary>
        public event EventHandler Updated;

        private IStorageAgent m_fileAgent;

        /// <summary>
        /// Returns all items with count != 0.
        /// </summary>
        public IEnumerable<InventoryCollectible> AllItems
        {
            get
            {
                return m_inventoryItems.
                    Where(kv => kv.Value.Count > 0).
                    Select(kv => new InventoryCollectible(
                        ScriptObjectDirectory.Instance.GetItem<Collectible>(kv.Key),
                        kv.Value.Count,
                        kv.Value.CollectTime)).
                    Where(i => i.Collectible != null);
            }
        }

        public Inventory()
        {
            m_inventoryItems = new Dictionary<string, InventoryItemState>();

            m_fileAgent = StorageManager.GetGameStorageAgent("inventory.json");

            var state = JsonHelper.Deserialize<InventoryState>(m_fileAgent);

            if (state != null && state.Items != null)
            {
                foreach (var cc in state.Items)
                {
                    m_inventoryItems[cc.CollectibleId] = cc;
                }
            }

            ScriptManager.Instance.ScriptsReset += Instance_ScriptsReset;
        }


        /// <summary>
        /// Returns player's items that emit an action.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="allCatalogItems"></param>
        /// <returns></returns>
        public IEnumerable<InventoryCollectible> GetItemsForAction(string action, bool includeAllItems = false)
        {
            var collectibles = CollectibleDirectory.Instance.GetCollectiblesEmitAction(action);
            return GetItems(collectibles, includeAllItems);
        }


        /// <summary>
        /// Get player's items that have at least one of the attributes specified. 
        /// </summary>
        /// <param name="attrs">List of atttributes. </param>
        /// <param name="allCatalogItems">Get all catalogs' items instead of player's items.</param>
        /// <returns></returns>
        public IEnumerable<InventoryCollectible> GetItemsWithAttributes(IEnumerable<string> attrs, bool includeAllItems = false)
        {
            if (includeAllItems)
            {
                var collectibles = CollectibleDirectory.Instance.GetCollectiblesWithAttribute(attrs);
                return GetItems(collectibles, includeAllItems);
            }
            else
            {
                return AllItems.Where(i => i.Collectible.HasAttributesOr(attrs));
            }
        }

        /// <summary>
        /// Get player's items that have at least one of the attributes specified. 
        /// </summary>
        /// <param name="attr">Attribute.</param>
        /// <param name="allCatalogItems">Get all catalogs' items instead of player's items.</param>
        /// <returns></returns>
        public IEnumerable<InventoryCollectible> GetItemsWithAttributes(string attr, bool includeAllItems = false)
        {
            return GetItemsWithAttributes(new string[] { attr }, includeAllItems);
        }

        /// <summary>
        /// Returns collectibles that can perform the specified action on the actee.
        /// </summary>
        /// <param name="actee"></param>
        /// <param name="action"></param>
        /// <param name="allCatalogItems">Player's items or all possible items?.</param>
        /// <returns></returns>
        public IEnumerable<InventoryCollectible> GetItemsForActionOnEntity(IActiveEntity actee, string action, bool includeAllCatalogItems = false)
        {
            var collectibles = CollectibleDirectory.Instance.GetItemsForActionOnEntity(actee, action);

            return GetItems(collectibles, includeAllCatalogItems);
        }

        /// <summary>
        /// Returns the counts of collectibles in the given catalog.
        /// </summary>
        /// <param name="catalogName"></param>
        /// <param name="includeAllCatalogItems">If true, includes items from the catalog even if their count is 0.</param>
        /// <returns></returns>
        public IEnumerable<InventoryCollectible> GetItems(IEnumerable<Collectible> collectibles, bool includeAllCatalogItems = false)
        {
            if (collectibles == null || collectibles.Count() <= 0)
            {
                return new InventoryCollectible[0];
            }

            var items = collectibles.Select(i =>
                new InventoryCollectible(
                i,
                GetCount(i.Id),
                GetCollectTime(i.Id)))
                                    .Where(i => includeAllCatalogItems || i.Count > 0)
                                    .ToArray();
            
            return items;
        }

        /// <summary>
        /// Returns the counts of collectibles in the given catalog.
        /// </summary>
        /// <param name="catalogName"></param>
        /// <param name="includeAllCatalogItems">If true, includes items from the catalog even if their count is 0.</param>
        /// <returns></returns>
        public IEnumerable<InventoryCollectible> GetItemsFromCatalog(string catalogName, bool includeAllCatalogItems = false)
        {
            if (includeAllCatalogItems)
            {
                IEnumerable<Collectible> collectibles;

                if (catalogName == null)
                {
                    collectibles = CollectibleDirectory.Instance.AllItems;
                }
                else
                {
                    collectibles = CollectibleDirectory.Instance.GetCatalogByName(catalogName);
                }

                if (collectibles != null)
                {
                    return GetItems(collectibles, includeAllCatalogItems);
                }
            }
            else
            {
                return m_inventoryItems
                    .Where(kv => kv.Value.Count > 0 &&
                        CollectibleDirectory.Instance.GetCatalogNameForItem(kv.Key) == catalogName)
                    .Select(kv =>
                        new InventoryCollectible(
                        CollectibleDirectory.Instance.GetItem(kv.Key),
                        kv.Value.Count,
                        kv.Value.CollectTime));
            }

            return null;
        }

        void Instance_ScriptsReset(object sender, EventArgs e)
        {
            m_inventoryItems.Clear();

            if (Updated != null)
            {
                Updated(this, EventArgs.Empty);
            }
        }

        public void Save()
        {
            var iis = m_inventoryItems.Values.ToArray();

            JsonHelper.Serialize(m_fileAgent, new InventoryState { Items = iis.ToArray() });
        }

        /// <summary>
        /// Returns the count of items with the specified collectibleId.
        /// </summary>
        /// <param name="collectibleId"></param>
        /// <returns></returns>
        public int GetCount(string collectibleId)
        {
            if (!string.IsNullOrEmpty(collectibleId) && m_inventoryItems.ContainsKey(collectibleId))
            {
                return m_inventoryItems[collectibleId].Count;
            }

            return 0;
        }

        /// <summary>
        /// Returns the count of items with the specified collectibleId.
        /// </summary>
        /// <param name="collectibleId"></param>
        /// <returns></returns>
        public DateTime? GetCollectTime(string collectibleId)
        {
            if (m_inventoryItems.ContainsKey(collectibleId))
            {
                return m_inventoryItems[collectibleId].CollectTime;
            }

            return null;
        }

        /// <summary>
        /// Removes count items from the inventory.
        /// </summary>
        /// <param name="collectibleId"></param>
        /// <param name="count"></param>
        /// <param name="commit">If true, commits the results to persistent storage.</param>
        public void Remove(string collectibleId, int count, bool commit = true)
        {
            if (m_inventoryItems.ContainsKey(collectibleId))
            {
                var item = m_inventoryItems[collectibleId];

                var curr = item.Count;

                var toTake = Math.Min(curr, count);

                item.Count -= toTake;

                var collectible = CollectibleDirectory.Instance.GetItem(collectibleId);

                // Archive items can't be removed entirely
                if (collectible != null && collectible.IsArchive)
                {
                    item.Count = Math.Max(item.Count, 1);
                }
                
                // If the count is zero this is no longer in our inventory.
                if (item.Count == 0)
                {
                    m_inventoryItems.Remove(collectibleId);
                }
            }

            if (Updated != null)
            {
                Updated(this, EventArgs.Empty);
            }

            if (commit)
            {
                Save();
            }
        }

        /// <summary>
        /// Removes a set of collectibles.
        /// </summary>
        /// <param name="collectibleCounts"></param>
        public void Remove(IEnumerable<CollectibleCount> collectibleCounts)
        {
            if (collectibleCounts != null)
            {
                foreach (var cc in collectibleCounts)
                {
                    Remove(cc, false);
                }
            }

            Save();
        }

        void Remove(CollectibleCount collectibleCount, bool commit)
        {
            Remove(collectibleCount.CollectibleId, collectibleCount.Count, commit);
        }

        /// <summary>
        /// Removes the count of collectibles from the inventory.
        /// </summary>
        /// <param name="collectibleCount"></param>
        public void Remove(CollectibleCount collectibleCount)
        {
            Remove(collectibleCount, true);
        }

        private void Add(string collectibleId, int count, bool commit)
        {
            if (collectibleId == null)
            {
                return;
            }

            InventoryItemState item = null;

            if (!m_inventoryItems.ContainsKey(collectibleId))
            {
                item = new InventoryItemState
                {
                    CollectibleId = collectibleId,
                    Count = count,
                    CollectTime = ClockManager.Instance.Now
                };

                m_inventoryItems[collectibleId] = item;
            }
            else
            {
                item = m_inventoryItems[collectibleId];

                item.Count += count;
                item.CollectTime = ClockManager.Instance.Now;
            }

            if (item.Limit.HasValue)
            {
                item.Count = Math.Min(item.Count, item.Limit.Value);
            }

            var collectible = CollectibleDirectory.Instance.GetItem(collectibleId);

            if (collectible != null && collectible.IsSingleton && item.Count > 1)
            {
                item.Count = 1;
            }
            
            if (Updated != null)
            {
                Updated(this, EventArgs.Empty);
            }

            if (commit)
            {
                Save();
            }
        }

        /// <summary>
        /// Sets the collectible count to the given value.
        /// </summary>
        /// <param name="collectibleCount"></param>
        /// <param name="commit">If true, commit the results to storage.</param>
        private void Set(CollectibleCount collectibleCount, bool commit)
        {
            var item = new InventoryItemState
            {
                CollectibleId = collectibleCount.CollectibleId,
                Count = collectibleCount.Count,
                CollectTime = ClockManager.Instance.Now
            };

            if (item.Limit.HasValue)
            {
                item.Count = Math.Min(item.Count, item.Limit.Value);
            }

            m_inventoryItems[collectibleCount.CollectibleId] = item;

            if (Updated != null)
            {
                Updated(this, EventArgs.Empty);
            }

            if (commit)
            {
                Save();
            }
        }

        private void Set(CollectibleCount collectibleCount)
        {
            Set(collectibleCount, true);
        }

        /// <summary>
        /// Adds the count of the specified collectible to the inventory.
        /// </summary>
        /// <param name="collectibleId"></param>
        /// <param name="qty"></param>
        public void Add(string collectibleId, int qty)
        {
            Add(collectibleId, qty, true);
        }

        /// <summary>
        /// Adds the count of the specified collectible to the inventory.
        /// </summary>
        /// </summary>
        /// <param name="collectibleCount"></param>
        public void Add(CollectibleCount collectibleCount)
        {
            Add(collectibleCount.CollectibleId, collectibleCount.Count, true);
        }

        /// <summary>
        /// Adds a set of collectibles to the inventory.
        /// </summary>
        /// <param name="collectibleCounts"></param>
        public void Add(IEnumerable<CollectibleCount> collectibleCounts)
        {
            if (collectibleCounts != null)
            {
                foreach (var cc in collectibleCounts)
                {
                    Add(cc);
                }
            }
        }

        /// <summary>
        /// Sets the collectible counts to the specified values.
        /// </summary>
        /// <param name="collectibleCounts"></param>
        public void Set(IEnumerable<CollectibleCount> collectibleCounts)
        {
            if (collectibleCounts != null)
            {
                foreach (var cc in collectibleCounts)
                {
                    Set(cc);
                }
            }
        }

        /// <summary>
        /// Returns the total number of items in the inventory.
        /// </summary>
        public int TotalNumberItems
        {
            get
            {
                return m_inventoryItems.Values.Select(v => v.Count).Sum();
            }
        }

        internal void SetLimits(InventoryCollectibleLimit limits)
        {
            if (limits.CollectibleCounts != null)
            {
                foreach (var limit in limits.CollectibleCounts)
                {
                    InventoryItemState item;

                    if (!m_inventoryItems.TryGetValue(limit.CollectibleId, out item))
                    {
                        item = new InventoryItemState
                        {
                            CollectibleId = limit.CollectibleId,
                            Limit = int.MaxValue
                        };

                        m_inventoryItems[item.CollectibleId] = item;
                    }

                    item.Limit = MathHelper.ApplyNumericalOperator(limits.Operator, item.Limit.GetValueOrDefault(), limit.Count);
                }
            }
        }
    }

}