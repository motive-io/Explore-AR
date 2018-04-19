// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Timing;
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays task information in a table.
    /// </summary>
    public partial class TaskItem : SelectableTableItem
    {
        public Text ItemNumber;
        public Text Title;
        public Text Subtitle;
        public Text Description;
        public GameObject ImageLayout;
        public RawImage Image;
        public GameObject NoImageFallback;
        public Button CompleteButton;
        public Button AcceptButton;
        public Text TimeLeft;
        public Table ActionItemTable;

        public TaskActionItem ActionItemPrefab;
        public TaskActionItem ActionItemCurrencyPrefab;
        public Color NeedsMoreTextColor = Color.red;

        public Table RewardItemTable;

        public TaskActionItem RewardItemCurrencyPrefab;
        public TaskActionItem RewardItemPrefab;

        public UnityEvent OnAction;

        public GameObject[] LinkedObjects;

        public GameObject[] ShowWhenNeedsItems;
        public GameObject[] ShowWhenHasItems;

        public GameObject[] ShowWhenComplete;
        public GameObject[] ShowWhenNotComplete;

        public IPlayerTaskDriver Driver { get; set; }

        protected override void Awake()
        {
            base.Awake();

            if (OnAction == null)
            {
                OnAction = new UnityEvent();
            }
        }

        public virtual void CompleteTask()
        {
            TaskManager.Instance.Complete(Driver);
        }

        public virtual void AcceptTask()
        {
            Driver.Accept();
            AcceptButton.gameObject.SetActive(false);
            CompleteButton.gameObject.SetActive(true);
        }

        public virtual void UserAction()
        {
            Driver.Action();

            if (OnAction != null)
            {
                OnAction.Invoke();
            }
        }

        protected virtual void Update()
        {
            if (TimeLeft)
            {
                if (Driver != null && Driver.TimeoutTimer != null)
                {
                    TimeLeft.gameObject.SetActive(true);

                    TimeSpan dt = Driver.TimeoutTimer.FireTime - ClockManager.Instance.Now;

                    if (dt.TotalSeconds < 0)
                    {
                        dt = TimeSpan.FromSeconds(0);
                    }

                    TimeLeft.text = string.Format("{0:00}:{1:00}:{2:00}", dt.Hours, dt.Minutes, dt.Seconds);
                }
                else
                {
                    TimeLeft.gameObject.SetActive(false);
                }
            }

            var hasItems = TaskManager.Instance.CanComplete(Driver);

            ObjectHelper.SetObjectsActive(ShowWhenHasItems, hasItems);
            ObjectHelper.SetObjectsActive(ShowWhenNeedsItems, !hasItems);
        }

        protected virtual void AddRewardCollectible(Table table, CollectibleCount cc)
        {
            var collectible = cc.Collectible;

            if (collectible == null)
            {
                return;
            }

            var prefab = GetRewardCollectiblePrefab(collectible);

            var aiObj = table.AddItem(prefab);

            if (aiObj.ItemName)
            {
                aiObj.ItemName.text = collectible.Title;
            }

            aiObj.Count.text = string.Format("{0}", cc.Count);
            aiObj.Id = cc.CollectibleId;

            if (collectible.ImageUrl != null)
            {
                ImageLoader.LoadImageOnThread(collectible.ImageUrl, aiObj.Image);
            }
        }

        protected virtual void AddGiveCollectible(Table table, CollectibleCount cc)
        {
            var collectible = cc.Collectible;

            if (collectible == null)
            {
                return;
            }

            var prefab = GetGiveCollectiblePrefab(collectible);

            var aiObj = table.AddItem(prefab);
            aiObj.Id = cc.CollectibleId;

            var invCount = Inventory.Instance.GetCount(cc.CollectibleId);

            if (aiObj.ItemName)
            {
                aiObj.ItemName.text = collectible.Title;
            }

            var satisfied = invCount >= cc.Count;

            aiObj.Count.text = StringFormatter.GetGiveCountString(invCount, cc.Count, NeedsMoreTextColor);

            if (collectible.ImageUrl != null)
            {
                ImageLoader.LoadImageOnThread(collectible.ImageUrl, aiObj.Image);
            }

            if (aiObj.SatisfiedObject)
            {
                aiObj.SatisfiedObject.SetActive(satisfied);
            }
        }

        protected virtual TaskActionItem GetGiveCurrencyPrefab(string currency)
        {
            return this.ActionItemCurrencyPrefab ?? this.ActionItemPrefab;
        }

        protected virtual TaskActionItem GetGiveCollectiblePrefab(Collectible collectible)
        {
            return this.ActionItemPrefab;
        }

        protected virtual TaskActionItem GetRewardCollectiblePrefab(Collectible collectible)
        {
            return this.RewardItemPrefab ?? this.ActionItemPrefab;
        }

        protected virtual TaskActionItem GetRewardCurrencyPrefab(string currency)
        {
            if (this.RewardItemCurrencyPrefab)
            {
                return this.RewardItemCurrencyPrefab;
            }
            else if (this.ActionItemCurrencyPrefab)
            {
                return this.ActionItemCurrencyPrefab;
            }

            return this.RewardItemPrefab ?? this.ActionItemPrefab;
        }

        protected virtual TaskActionItem GetTakeCurrencyPrefab(string currency)
        {
            return this.ActionItemCurrencyPrefab ?? this.ActionItemPrefab;
        }

        protected virtual TaskActionItem GetTakeCollectiblePrefab(Collectible collectible)
        {
            return this.ActionItemPrefab;
        }

        protected virtual void AddGiveCurrency(Table table, CurrencyCount cc)
        {
            var prefab = GetGiveCurrencyPrefab(cc.Currency);

            if (!prefab)
            {
                return;
            }

            var aiObj = table.AddItem(prefab);

            var invCount = Wallet.Instance.GetCount(cc.Currency);

            if (aiObj.ItemName)
            {
                aiObj.ItemName.text = cc.Currency;
            }

            aiObj.Count.text = StringFormatter.GetGiveCountString(invCount, cc.Count, NeedsMoreTextColor);

            /*
             * TODO: dynamic lookup of currency
            if (collectible.ImageUrl != null)
            {
                ThreadHelper.Instance.StartCoroutine(ImageLoader.LoadImage(collectible.ImageUrl, aiObj.Image));
            }*/

            if (aiObj.SatisfiedObject)
            {
                aiObj.SatisfiedObject.SetActive(invCount >= cc.Count);
            }
        }

        protected virtual void AddTakeCurrency(Table table, CurrencyCount cc)
        {
            var prefab = GetGiveCurrencyPrefab(cc.Currency);

            if (!prefab)
            {
                return;
            }

            var aiObj = table.AddItem(prefab);

            if (aiObj.ItemName)
            {
                aiObj.ItemName.text = cc.Currency;
            }

            aiObj.Count.text = string.Format("{0}", cc.Count);

            /*
             * TODO: dynamic lookup of currency
            if (collectible.ImageUrl != null)
            {
                ThreadHelper.Instance.StartCoroutine(ImageLoader.LoadImage(collectible.ImageUrl, aiObj.Image));
            }*/
        }

        protected virtual void AddRewardCurrency(Table table, CurrencyCount cc)
        {
            var prefab = GetRewardCurrencyPrefab(cc.Currency);

            if (!prefab)
            {
                return;
            }

            var aiObj = table.AddItem(prefab);

            if (aiObj.ItemName)
            {
                aiObj.ItemName.text = cc.Currency;
            }

            aiObj.Count.text = string.Format("{0}", cc.Count);

            /*
             * TODO: dynamic lookup of currency
            if (collectible.ImageUrl != null)
            {
                ThreadHelper.Instance.StartCoroutine(ImageLoader.LoadImage(collectible.ImageUrl, aiObj.Image));
            }*/
        }

        protected virtual void PopulateRewardItemTable(Table table, ValuablesCollection valuables)
        {
            if (!table)
            {
                return;
            }

            table.Clear();

            if (valuables != null)
            {
                if (valuables.CollectibleCounts != null)
                {
                    foreach (var cc in valuables.CollectibleCounts)
                    {
                        AddRewardCollectible(table, cc);
                    }
                }

                if (valuables.CurrencyCounts != null)
                {
                    foreach (var cc in valuables.CurrencyCounts)
                    {
                        AddRewardCurrency(table, cc);
                    }
                }
            }
        }

        protected virtual void PopulateGiveActionItemTable(Table table, ValuablesCollection valuables)
        {
            var task = Driver.Task;

            if (table)
            {
                table.Clear();

                if (valuables != null)
                {
                    if (valuables.CollectibleCounts != null)
                    {
                        foreach (var cc in valuables.CollectibleCounts)
                        {
                            AddGiveCollectible(table, cc);
                        }
                    }

                    if (valuables.CurrencyCounts != null)
                    {
                        foreach (var cc in valuables.CurrencyCounts)
                        {
                            AddGiveCurrency(table, cc);
                        }
                    }
                }
            }
        }

        protected virtual void PopulateTakeActionItemTable(Table table, ValuablesCollection valuables)
        {
            var task = Driver.Task;

            if (table)
            {
                table.Clear();

                if (valuables != null && valuables.CollectibleCounts != null)
                {
                    foreach (var cc in valuables.CollectibleCounts)
                    {
                        if (cc.Collectible == null)
                        {
                            continue;
                        }

                        var collectible = cc.Collectible;
                        // todo..
                        var prefab = GetTakeCollectiblePrefab(collectible);

                        var aiObj = table.AddItem(prefab);

                        var invCount = Inventory.Instance.GetCount(cc.CollectibleId);
                        aiObj.ItemName.text = collectible.Title;
                        aiObj.Count.text = string.Format("{0}", cc.Count);
                        aiObj.Id = collectible.Id;

                        if (collectible.ImageUrl != null)
                        {
                            ImageLoader.LoadImageOnThread(collectible.ImageUrl, aiObj.Image);
                        }

                        if (aiObj.SatisfiedObject)
                        {
                            aiObj.SatisfiedObject.SetActive(invCount >= cc.Count);
                        }
                    }
                }


                if (valuables != null && valuables.CurrencyCounts != null)
                {
                    foreach (var cc in valuables.CurrencyCounts)
                    {
                        AddTakeCurrency(table, cc);
                    }
                }
            }
        }

        protected virtual void SetCanComplete(bool canComplete)
        {
            if (CompleteButton)
            {
                CompleteButton.interactable = canComplete;
            }
        }

        public virtual void Populate(IPlayerTaskDriver driver)
        {
            Driver = driver;
            Populate();
            PopulateComponents(driver);
        }

        public virtual void Populate()
        {
            if (!ActionItemTable)
            {
                ActionItemTable = GetComponentInChildren<Table>();
            }

            if (ActionItemTable)
            {
                ActionItemTable.Clear();
            }

            var task = Driver.Task;

            ObjectHelper.SetObjectsActive(ShowWhenComplete, Driver.IsComplete);
            ObjectHelper.SetObjectsActive(ShowWhenNotComplete, !Driver.IsComplete);

            if (Title)
            {
                Title.text = task.Title;
            }

            if (Description)
            {
                Description.text = task.Description;
            }

            PopulateImage(Image, task.ImageUrl);
            SetCanComplete(TaskManager.Instance.CanComplete(Driver));

            if (TaskManager.Instance.IsGiveTask(task) && ActionItemPrefab)
            {
                PopulateGiveActionItemTable(ActionItemTable, task.ActionItems);
            }
            else if (TaskManager.Instance.IsTakeTask(task) && ActionItemPrefab)
            {
                PopulateTakeActionItemTable(ActionItemTable, task.ActionItems);
            }

            if (task.Reward != null)
            {
                PopulateRewardItemTable(RewardItemTable, task.Reward);
            }
        }

        public virtual void PopulateImage(RawImage image, string url)
        {
            if (url != null)
            {
                ObjectHelper.SetObjectActive(NoImageFallback, false);
                ObjectHelper.SetObjectActive(ImageLayout, true);
                ObjectHelper.SetObjectActive(Image, true);

                ImageLoader.LoadImageOnThread(url, Image);
            }
            else
            {
                ObjectHelper.SetObjectActive(Image, false);
                ObjectHelper.SetObjectActive(NoImageFallback, true);
                ObjectHelper.SetObjectActive(ImageLayout, false);
            }
        }

        public virtual void SetSelected(bool selected)
        {
            ObjectHelper.SetObjectsActive(LinkedObjects, selected);
        }

        public void CallOnAction()
        {
            if (OnAction == null) return;

            OnAction.Invoke();
            OnAction = null;
        }

        public virtual void Reset()
        {
            if (Image)
            {
                Image.texture = null;
            }

            if (ActionItemTable)
            {
                ActionItemTable.Clear();
            }

            if (RewardItemTable)
            {
                RewardItemTable.Clear();
            }
        }

        public void SetText(Text textObj, string value)
        {
            if (textObj)
            {
                textObj.text = value;
            }
        }
    }
}