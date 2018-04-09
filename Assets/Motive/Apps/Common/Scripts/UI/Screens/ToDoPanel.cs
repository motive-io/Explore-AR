// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Apps.Attractions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.UI
{
    public class ToDoPanel : TablePanel
    {
        public bool ShowTasks = true;
        public bool ShowAttractions = true;

        public PanelLink AttractionDetailsPanel;

        public LocationAttractionItem LocationAttractionItem;

        public override void Populate()
        {
            Table.Clear();

            foreach (var attraction in AttractionManager.Instance.ActiveAttractions)
            {
                var arg = attraction;

                var item = AddSelectableItem(LocationAttractionItem, (_item) =>
                    {
                        if (AttractionDetailsPanel)
                        {
                            AttractionDetailsPanel.Push(arg);
                        }
                    });

                item.Populate(attraction);
            }
        }
    }
}