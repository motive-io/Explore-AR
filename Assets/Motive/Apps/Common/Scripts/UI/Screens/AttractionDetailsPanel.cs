// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Attractions.Models;
using Motive.Core.Models;
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.Unity.Apps.Attractions;
using Motive.Unity.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    public class AttractionDetailsPanel : TablePanel<ActivatedAttractionContext>
    {
        public Text Title;
        public Text Description;
        public RawImage Image;

        public AttractionContentItem AttractionContentItem;
        public AttractionInteractibleItem AttractionInteractibleItem;

        public PanelLink ScreenMessagePanelLink;
        public PanelLink AudioPanelLink;
        public PanelLink VideoPanelLink;

        private void PopulateInteractible(ActiveResourceContainer<LocationAttractionItemProperties> inter)
        {
            var isUnlocked = Data.IsUnlocked(inter.ActivationContext, inter.Resource);

            var item = AddSelectableItem(AttractionInteractibleItem, (_item) =>
            {

            });

            item.Selectable = isUnlocked;

            item.SetText(inter.Resource.Title);
            item.SetText(item.Description, inter.Resource.Description);

            item.Locked.SetActive(!isUnlocked);
            item.Unlocked.SetActive(isUnlocked);
        }

        void PopulateContent(ActiveResourceContainer<LocationAttractionItemProperties> attractionContent)
        {
            var isUnlocked = Data.IsUnlocked(attractionContent.ActivationContext, attractionContent.Resource);

            PanelLink toPush = null;

            var contentItem = attractionContent.Resource.GetItem();

            if (contentItem is ScreenMessage)
            {
                toPush = ScreenMessagePanelLink;
            }
            else if (contentItem is MediaContent)
            {
                toPush = AudioPanelLink;
            }

            var item = AddSelectableItem(AttractionContentItem, (_item) =>
            {
                if (toPush)
                {
                    toPush.Push(contentItem);
                }
            });

            item.Selectable = isUnlocked;

            item.SetText(attractionContent.Resource.Title);
            item.SetText(item.Description, attractionContent.Resource.Description);

            item.Locked.SetActive(!isUnlocked);
            item.Unlocked.SetActive(isUnlocked);
        }

        public override void Populate(ActivatedAttractionContext data)
        {
            Table.Clear();

            if (Title)
            {
                Title.text = data.Attraction.Title;
            }

            if (Description)
            {
                Description.text = data.Attraction.Description;
            }

            if (Image)
            {
                ImageLoader.LoadImageOnThread(data.Attraction.ImageUrl, Image);
            }

            var handlers = data.ItemHandlers;

            if (handlers != null)
            {
                foreach (var handler in handlers)
                {
                    //PopulateItem(handler);
                }
            }

            base.Populate(data);
        }

    }
}