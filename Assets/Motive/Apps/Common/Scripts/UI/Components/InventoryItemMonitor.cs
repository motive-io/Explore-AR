// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Motive.UI.Framework;
using Motive.Unity.Apps;
using Motive.Unity.Gaming;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Monitors inventory and updates a UI table.
    /// </summary>
    public class InventoryItemMonitor : MonoBehaviour
    {
        public bool ShowAllItems;
        public string CatalogName;
        public string ItemAttribute;
        public Table Table;

        public InventoryTableItem ItemPrefab;

        Dictionary<string, InventoryTableItem> m_existingItems;

        void Awake()
        {
            m_existingItems = new Dictionary<string, InventoryTableItem>();

            if (!Table)
            {
                Table = GetComponentInChildren<Table>();
            }

            Table.Clear();

            AppManager.Instance.OnLoadComplete(() =>
            {
                Inventory.Instance.Updated += Inventory_Updated;

                Populate();
            });
        }

        void Inventory_Updated(object sender, System.EventArgs e)
        {
            Populate();
        }

        void Populate()
        {
            IEnumerable<InventoryCollectible> items = null;
            IEnumerable<InventoryCollectible> catItems = null;

            if (!ShowAllItems)
            {
                Table.Clear();
                m_existingItems.Clear();
            }

            if (!string.IsNullOrEmpty(CatalogName))
            {
                catItems = Inventory.Instance.GetItemsFromCatalog(CatalogName, ShowAllItems);
            }
            else if (!string.IsNullOrEmpty(ItemAttribute))
            {
                catItems = Inventory.Instance.GetItemsWithAttributes(ItemAttribute, ShowAllItems);
            }

            if (catItems != null)
            {
                items = catItems != null ? catItems.OrderBy(_i => _i.Collectible.InventoryOrder.GetValueOrDefault(99999)) : catItems;
            }
            else
            {
                items = Inventory.Instance.AllItems;
            }

            int itemIdx = 0;

            if (items == null) return;

            foreach (var item in items)
            {
                if (item.Collectible != null)
                {
                    InventoryTableItem obj = null;

                    m_existingItems.TryGetValue(item.Collectible.Id, out obj);

                    if (!obj)
                    {
                        obj = Table.AddItem<InventoryTableItem>(ItemPrefab);
                        m_existingItems[item.Collectible.Id] = obj;
                    }

                    obj.Populate(item);

                    itemIdx++;
                }
            }
        }
    }
}