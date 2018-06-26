// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays a reward or rewards given to the player. Is a singleton.
    /// </summary>
    public class RewardPanel : Panel<ValuablesCollection>
    {
        public static RewardPanel Instance { get; private set; }

        public Text DescriptionText;

        public TextImageItem CurrencyItemPrefab;
        public InventoryTableItem CollectibleItemPrefab;

        public GameObject Collectibles;
        public GameObject Currencies;

        public Table CollectiblesTable;
        public Table CurrenciesTable;

        public int MaximumItems = 1;

        protected override void Awake()
        {
            base.Awake();

            Instance = this;

            if (!CollectiblesTable && Collectibles)
            {
                CollectiblesTable = Collectibles.GetComponentInChildren<Table>();
            }

            if (!CurrenciesTable && Currencies)
            {
                CurrenciesTable = Currencies.GetComponentInChildren<Table>();
            }
        }

        void Instance_RewardAdded(object sender, RewardEventArgs e)
        {
            PanelManager.Instance.Push(this, e.Valuables);
        }

        public override void ClearData()
        {
            if (CollectiblesTable)
            {
                CollectiblesTable.Clear();
            }

            if (CurrenciesTable)
            {
                CurrenciesTable.Clear();
            }

            ObjectHelper.SetObjectActive(Collectibles, false);
            ObjectHelper.SetObjectActive(Currencies, false);

            base.ClearData();
        }

        public override void Populate(ValuablesCollection data)
        {
            if (CollectiblesTable)
            {
                CollectiblesTable.Clear();
            }

            if (CurrenciesTable)
            {
                CurrenciesTable.Clear();
            }

            bool hasCollectibles = false;
            bool hasCurrencies = false;

            if (data != null)
            {
                if (data.CollectibleCounts != null && CollectiblesTable)
                {
                    hasCollectibles = true;

                    int count = 0;

                    foreach (var cc in data.CollectibleCounts)
                    {
                        var collectible = cc.Collectible;

                        if (collectible != null)
                        {
                            var item = CollectiblesTable.AddItem(CollectibleItemPrefab);

                            var ic = new InventoryCollectible(collectible, cc.Count);

                            item.Populate(ic);

                            if (++count > MaximumItems) break;
                        }
                    }
                }

                if (data.CurrencyCounts != null && CurrenciesTable)
                {
                    hasCurrencies = true;

                    foreach (var cc in data.CurrencyCounts)
                    {
                        var item = CurrenciesTable.AddItem(CurrencyItemPrefab);

                        item.SetText("+ " + cc.Count.ToString() + " " + cc.Currency);
                    }
                }
            }

            ObjectHelper.SetObjectActive(Collectibles, hasCollectibles);
            ObjectHelper.SetObjectActive(Currencies, hasCurrencies);

            base.Populate(data);
        }

        public static void Show(ValuablesCollection rewards)
        {
            if (rewards != null)
            {
                PanelManager.Instance.Push(Instance, rewards);
            }
        }
    }

}