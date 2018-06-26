// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays the items in a player's inventory. Can either show
    /// only items in the inventory or have blank spaces for items
    /// that still need to be collected.
    /// </summary>
    public class InventoryPanel : TablePanel
    {
        public string[] ItemAttributesFilter;

        public InventoryTableItem InventoryItem;

        public string CatalogName;
        public bool ShowAllItems;
        public bool PrePopulated;
        public bool AutoSelectItem;

        private ExclusiveUpdater m_updater;
        private string m_selectedItemId;

        protected override void Awake()
        {
            m_updater = new ExclusiveUpdater();
            base.Awake();
        }

        protected virtual void SelectItem(InventoryTableItem item)
        {
            PopulateComponent<InventoryDetailsComponent>(item);
        }

        public virtual IEnumerable<InventoryCollectible> FilterItems(IEnumerable<InventoryCollectible> input)
        {

            return input;
        }

        public override void DidShow()
        {
            Inventory.Instance.Updated += Instance_Updated;
            base.DidShow();
        }

        public override void DidHide()
        {
            Inventory.Instance.Updated -= Instance_Updated;

            base.DidHide();
        }

        void Instance_Updated(object sender, System.EventArgs e)
        {
            m_updater.Update(Populate);
        }

        public override void Populate()
        {
            if (!PrePopulated)
            {
                Table.Clear();
            }
            else
            {
                foreach (var item in Table.GetComponentsInChildren<InventoryTableItem>())
                {
                    if (item.EmptyObject)
                    {
                        item.EmptyObject.SetActive(true);
                    }

                    if (item.PopulatedObject)
                    {
                        item.PopulatedObject.SetActive(false);
                    }
                }
            }

            IEnumerable<InventoryCollectible> items = null;

            if (ItemAttributesFilter != null && ItemAttributesFilter.Count() > 0)
            {
                items = Inventory.Instance.GetItemsWithAttributes(ItemAttributesFilter, ShowAllItems);
            }
            else if (ShowAllItems)
            {
                items = Inventory.Instance.GetItems(CollectibleDirectory.Instance.AllItems);
            }
            else
            {
                items = Inventory.Instance.AllItems;
            }
            items = FilterItems(items);

            int itemIdx = 0;

            InventoryTableItem selectItem = null;

            if (PrePopulated)
            {
                for (int i = 0; i < Table.transform.childCount; i++)
                {
                    var item = Table.transform.GetChild(i).GetComponent<InventoryTableItem>();

                    if (item)
                    {
                        item.OnSelected.RemoveAllListeners();

                        item.PopulatedObject.SetActive(false);
                    }
                }
            }

            foreach (var item in items)
            {
                if (item.Collectible != null)
                {
                    InventoryTableItem obj = null;

                    if (PrePopulated)
                    {
                        int idx = item.Collectible.InventoryOrder.GetValueOrDefault(itemIdx);

                        if (idx >= Table.transform.childCount)
                        {
                            continue;
                        }

                        obj = Table.transform.GetChild(idx).gameObject.GetComponent<InventoryTableItem>();

                        if (!obj)
                        {
                            return;
                        }

                        obj.OnSelected.AddListener(() => { SelectItem(obj); });
                    }
                    else
                    {
                        obj = AddSelectableItem(InventoryItem, (_item) => { SelectItem(_item); });
                    }

                    obj.Populate(item);

                    itemIdx++;

                    if (selectItem == null)
                    {
                        selectItem = obj;
                    }
                    else if (item.Collectible.Id == m_selectedItemId)
                    {
                        selectItem = obj;
                    }
                }

                if (selectItem != null && AutoSelectItem)
                {
                    m_selectedItemId = selectItem.Collectible.Id;

                    selectItem.Select();
                }
            }
        }

        public override void Reset()
        {
            if (!PrePopulated)
            {
                base.Reset();
            }
        }
    }
}