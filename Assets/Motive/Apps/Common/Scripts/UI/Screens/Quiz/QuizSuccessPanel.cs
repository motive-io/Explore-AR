// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    public class QuizSuccessPanel : Panel<PlayerTaskDriver>
    {

        public static QuizSuccessPanel Instance { get; private set; }

        public Text DescriptionText;

        public InventoryTableItem CollectibleItemPrefab;

        public GameObject RewardCollectibles;
        public Table RewardCollectiblesTable;


        public GameObject GiveCollectibles;
        public Table GiveCollectiblesTable;

        public int MaximumItems = 1;

        protected override void Awake()
        {
            base.Awake();

            Instance = this;

            if (!RewardCollectiblesTable && RewardCollectibles)
            {
                RewardCollectiblesTable = RewardCollectibles.GetComponentInChildren<Table>();
            }

            if (!GiveCollectiblesTable && GiveCollectibles)
            {
                GiveCollectiblesTable = GiveCollectibles.GetComponentInChildren<Table>();
            }

            //RewardManager.Instance.RewardPanel = this;
        }

        //void Instance_RewardAdded(object sender, RewardEventArgs e)
        //{
        //    PanelManager.Instance.Push(this, e.Valuables);
        //}

        public override void Populate(PlayerTaskDriver data)
        {
            if (RewardCollectiblesTable)
            {
                RewardCollectiblesTable.Clear();
            }

            if (GiveCollectiblesTable)
            {
                GiveCollectiblesTable.Clear();
            }

            var rewardItems = new List<CollectibleCount>();
            var hasReward = false;

            var giveItems = new List<CollectibleCount>();
            var hasGive = false;

            if (data != null)
            {
                if (Data.IsGiveTask
                    && Data.Task.ActionItems != null
                    && Data.Task.ActionItems.CollectibleCounts != null
                    && Data.Task.ActionItems.CollectibleCounts.Count() > 0)
                {
                    giveItems.AddRange(Data.Task.ActionItems.CollectibleCounts);
                }

                if (Data.IsTakeTask
                    && Data.Task.ActionItems != null
                    && Data.Task.ActionItems.CollectibleCounts != null
                    && Data.Task.ActionItems.CollectibleCounts.Count() > 0)
                {
                    rewardItems.AddRange(Data.Task.ActionItems.CollectibleCounts);
                }

                if (Data.Task.Reward != null
                    && Data.Task.Reward.CollectibleCounts != null
                    && Data.Task.Reward.CollectibleCounts.Count() > 0)
                {
                    rewardItems.AddRange(Data.Task.Reward.CollectibleCounts);
                }


                //// populate tables
                // give items
                if (giveItems.Count() > 0 && GiveCollectiblesTable && GiveCollectibles)
                {
                    hasGive = true;
                    int displayCount = 0;
                    foreach (var cc in giveItems)
                    {
                        var collectible = cc.Collectible;
                        if (collectible != null)
                        {
                            var item = GiveCollectiblesTable.AddItem(CollectibleItemPrefab);

                            var ic = new InventoryCollectible(collectible, cc.Count);

                            item.Populate(ic);

                            if (++displayCount > MaximumItems) break;
                        }
                    }
                }
                // reward items
                if (rewardItems.Count() > 0 && RewardCollectiblesTable && RewardCollectibles)
                {
                    hasReward = true;
                    int displayCount = 0;
                    foreach (var cc in rewardItems)
                    {
                        var collectible = cc.Collectible;
                        if (collectible != null)
                        {
                            var item = RewardCollectiblesTable.AddItem(CollectibleItemPrefab);

                            var ic = new InventoryCollectible(collectible, cc.Count);

                            item.Populate(ic);

                            if (++displayCount > MaximumItems) break;
                        }
                    }
                }

            }

            ObjectHelper.SetObjectActive(RewardCollectibles, hasReward);
            ObjectHelper.SetObjectActive(GiveCollectibles, hasGive);

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