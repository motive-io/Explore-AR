// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Table item for displaying inventory items.
    /// </summary>
    public class InventoryTableItem : CollectibleTableItem
    {
        public GameObject EmptyObject;
        public GameObject PopulatedObject;
        public GameObject NumberLayout;
        public Text NumberText;
        public Text DateText;

        public bool HideCountIfZero;
        public bool UseColorIfZero;

        public UnityEngine.Color NoItemColor = new UnityEngine.Color()
        {
            r = .62f,
            b = .59f,
            g = .62f,
            a = .69f
        };

        public Button OpenItem;
        public InventoryCollectible InventoryCollectible { get; private set; }

        public virtual void Populate(InventoryCollectible invCollectible)
        {
            if (PopulatedObject)
            {
                PopulatedObject.SetActive(true);
            }

            this.InventoryCollectible = invCollectible;
            var collectible = invCollectible.Collectible;
            var count = invCollectible.Count;

            ObjectHelper.SetObjectActive(NumberLayout, !collectible.IsSingleton);

            if (NumberText)
            {
                if (count > 0 || !HideCountIfZero)
                {
                    NumberText.text = count.ToString();
                }
                else
                {
                    NumberText.gameObject.SetActive(false);
                }
            }

            if (UseColorIfZero && Image && count == 0)
            {
                Image.color = NoItemColor;
            }

            base.Populate(collectible);
        }
    }

}