// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Used for displaying inventory item details.
    /// </summary>
    public class InventoryCollectiblePanel : Panel<InventoryCollectible>
    {
        public Text Title;
        public Text Description;
        public Text Date;
        public RawImage Image;

        protected virtual string GetImageUrl(Collectible collectible)
        {
            return collectible.ImageUrl;
        }

        public virtual void PopulateContent(Collectible collectible)
        {
            PopulateComponent<MediaContentComponent>(collectible.Content);
        }

        public override void Populate(InventoryCollectible data)
        {
            var collectible = data.Collectible;

            if (Title)
            {
                Title.text = collectible.Title;
            }

            if (Description)
            {
                Description.text = !string.IsNullOrEmpty(collectible.Description) ? collectible.Description : "";
            }

            if (Date)
            {
                var collectTime = Inventory.Instance.GetCollectTime(collectible.Id);
                Date.text = collectTime != null ? collectTime.Value.ToShortDateString() : "";// data.InventoryCollectible.CollectTime.GetValueOrDefault().ToString();
            }

            if (Image)
            {
                Image.texture = null;

                ImageLoader.LoadImageOnThread(GetImageUrl(collectible), Image);
            }

            PopulateContent(data.Collectible);

            base.Populate(data);
        }
    }

}