// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.UI.Framework;
using Motive.Unity.Apps;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Panel that lets the user select from a list of location types.
    /// </summary>
    public class LocationCategorySelectPanel : TablePanel
    {
        public LocationCategorySelectItem ItemPrefab;

        public bool IsSelectAll { get; private set; }

        public LocationCategory SelectedCategory { get; private set; }

        public override void Populate()
        {
            IsSelectAll = false;
            SelectedCategory = null;

            Table.Clear();

            foreach (var cat in LocationCategoryDirectory.Instance.AllItems)
            {
                var item = Table.AddItem(ItemPrefab);

                item.SetText(item.Text, cat.Title);
                item.SetText(item.Description, cat.Description);

                // Local reference
                var _cat = cat;

                item.OnSelected.AddListener(() =>
                    {
                        SelectedCategory = _cat;

                        Back();
                    });
            }

            base.Populate();
        }

        public void SelectAll()
        {
            IsSelectAll = true;

            Back();
        }
    }

}