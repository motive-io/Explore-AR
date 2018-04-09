// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.UI.Framework
{
    /// <summary>
    /// A panel that manages a Table.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TablePanel<T> : Panel<T>
    {
        public Table Table;
        public ScrollRect ScrollRect { get; set; }

        protected SelectableTableItem SelectedItem { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            if (!Table)
            {
                Table = GetComponentInChildren<Table>(true);
            }

            if (!ScrollRect)
            {
                ScrollRect = GetComponentInChildren<ScrollRect>(true);
            }
        }

        protected virtual void SetSelectedItem(SelectableTableItem item)
        {
            if (SelectedItem)
            {
                if (SelectedItem.EnabledWhenNotSelected)
                {
                    SelectedItem.EnabledWhenNotSelected.SetActive(true);
                }

                if (SelectedItem.EnabledWhenSelected)
                {
                    SelectedItem.EnabledWhenSelected.SetActive(false);
                }
            }

            SelectedItem = item;

            if (SelectedItem)
            {
                if (SelectedItem.EnabledWhenNotSelected)
                {
                    SelectedItem.EnabledWhenNotSelected.SetActive(false);
                }

                if (SelectedItem.EnabledWhenSelected)
                {
                    SelectedItem.EnabledWhenSelected.SetActive(true);
                }
            }
        }

        public virtual I AddSelectableItem<I>(I prefab, Action<I> onSelect = null) where I : SelectableTableItem
        {
            var item = Table.AddItem(prefab);

            if (item.EnabledWhenNotSelected)
            {
                item.EnabledWhenNotSelected.SetActive(true);
            }

            if (item.EnabledWhenSelected)
            {
                item.EnabledWhenSelected.SetActive(false);
            }

            item.OnSelected.AddListener(() => 
            {
                SetSelectedItem(item);

                if (onSelect != null)
                {
                    onSelect(item);
                }
            });

            return item;
        }

        protected virtual void OnSelectItem(SelectableTableItem item)
        {

        }

		public override void DidHide ()
		{
            Reset();

			Resources.UnloadUnusedAssets();

			base.DidHide ();
		}

        public virtual void Reset()
        {
            Table.Clear();

            ScrollRect.normalizedPosition = Vector2.zero;
        }
    }

    public class TablePanel : TablePanel<object>
    {

    }
}