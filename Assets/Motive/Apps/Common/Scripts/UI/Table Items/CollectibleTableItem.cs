// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Table item for displaying collectible information.
    /// </summary>
    public class CollectibleTableItem : TextImageItem
    {
        public Collectible Collectible { get; private set; }

        public virtual void Populate(Collectible collectible)
        {
            this.Collectible = collectible;

            SetText(collectible.Title);
            SetImage(collectible.ImageUrl);
        }
    }
}