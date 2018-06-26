// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Base class for displaying task info from an IPlayerTaskDriver.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TaskInfoComponent<T> : PanelComponent<T>
        where T : IPlayerTaskDriver
    {
        public GameObject TitleLayout;
        public Text Title;
        public GameObject DescriptionLayout;
        public Text Description;
        public GameObject ActionButtonLayout;
        public Button ActionButton;
        public Text ActionButtonText;

        public GameObject ImageLayout;
        public RawImage Image;

        [Tooltip("Show these items when the player has satisfied the action item requirements for this task. Hide when the player cannot satisfy.")]
        public GameObject[] ShowWhenCanComplete;
        [Tooltip("Show these items when the player has not satisfied the action item requirements for this task. Hide when the player does satisfy.")]
        public GameObject[] ShowWhenCannotComplete;

        public Table CollectiblesTable;
        public InventoryTableItem TableItemPrefab;

        public void Action()
        {
            Data.Action();
        }

        public override void Populate(T obj)
        {
            SetText(TitleLayout, Title, obj.Task.Title);
            SetText(DescriptionLayout, Description, obj.Task.Description);
            SetImage(ImageLayout, Image, obj.Task.ImageUrl);

            base.Populate(obj);

            ShowObjectsIfCanComplete();
            if (obj.IsGiveTask && !TaskManager.Instance.CanComplete(obj))
            {
                PopulateActionItemsTable();
            }
        }

        private void PopulateActionItemsTable()
        {
            if (!CollectiblesTable || !TableItemPrefab)
            {
                return;
            }

            CollectiblesTable.Clear();

            if (Data != null && Data.Task.ActionItems != null)
            {
                if (Data.Task.ActionItems.CollectibleCounts != null && CollectiblesTable)
                {
                    foreach (var cc in Data.Task.ActionItems.CollectibleCounts)
                    {
                        var collectible = cc.Collectible;

                        if (collectible != null)
                        {
                            var playerInvCount = Inventory.Instance.GetCount(cc.CollectibleId);

                            var item = CollectiblesTable.AddItem(TableItemPrefab);

                            item.Populate(collectible);

                            // If the item doesn't have Number Text, let's make sure to 
                            // add the number to the main text box.
                            if (!item.NumberText)
                            {
                                string text = string.Format("{0}x {1}", cc.Count, collectible.Title);

                                item.SetText(text);
                            }

                            item.NumberText.text = string.Format("{0} / {1}", playerInvCount, cc.Count);

                            //if (playerInvCount >= cc.Count)
                            //{
                            //}

                        }
                    }
                }

                //if (Data.CurrencyCounts != null && CurrenciesTable)
                //{
                //    hasCurrencies = true;

                //    foreach (var cc in Data.CurrencyCounts)
                //    {
                //        var item = CurrenciesTable.AddItem(CurrencyItemPrefab);

                //        item.SetText("+ " + cc.Count.ToString() + " " + cc.Currency);
                //    }
                //}
            }

        }

        public void ShowCanCompleteObjects(bool doShow = true)
        {
            ObjectHelper.SetObjectsActive(ShowWhenCanComplete, doShow);
            ObjectHelper.SetObjectsActive(ShowWhenCannotComplete, !doShow);
        }

        /// <summary>
        /// For any task, hide/show objects if the player needs more items.
        /// </summary>
        public void ShowObjectsIfCanComplete()
        {
            var driver = this.Data;

            // if missing any parameters, just act like the collectible requirement is satisfied
            if (driver == null || driver.Task == null || string.IsNullOrEmpty(driver.Task.Action))
            {
                ShowCanCompleteObjects(true);
                return;
            }

            if (!driver.IsGiveTask)
            {
                ShowCanCompleteObjects(true);
                return;
            }

            if (TaskManager.Instance.CanComplete(driver))
            {
                ShowCanCompleteObjects(true);
            }
            else
            {
                ShowCanCompleteObjects(false);
            }
        }


        private void Inventory_Updated(object sender, EventArgs e)
        {
            ShowObjectsIfCanComplete();
        }

        public override void DidShow()
        {
            base.DidShow();

            Inventory.Instance.Updated += Inventory_Updated;

        }
        public override void DidHide()
        {
            base.DidHide();

            Inventory.Instance.Updated -= Inventory_Updated;
        }


    } 
}
