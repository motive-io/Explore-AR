// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Media;
using Motive.Core.Models;
using Motive.Gaming.Models;
using Motive.UI.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Used with an inventory panel to show details for an inventory item.
    /// </summary>
    public class InventoryDetailsComponent : PanelComponent<InventoryTableItem>
    {
        public Panel InventoryAudioPanel;
        public Panel InventoryVideoPanel;
        public Panel InventoryImagePanel;
        public Panel InventoryScreenMessagePanel;
        public Panel DefaultInventoryPanel;

        public PanelContainer ItemViewContainer;

        void Start()
        {
            if (!InventoryAudioPanel)
            {
                InventoryAudioPanel = PanelManager.Instance.GetPanel<InventoryAudioPanel>();
            }

            if (!InventoryVideoPanel)
            {
                InventoryVideoPanel = PanelManager.Instance.GetPanel<InventoryVideoPanel>();
            }

            if (!InventoryImagePanel)
            {
                InventoryImagePanel = PanelManager.Instance.GetPanel<InventoryImagePanel>();
            }

            if (!InventoryScreenMessagePanel)
            {
                InventoryScreenMessagePanel = PanelManager.Instance.GetPanel<InventoryScreenMessagePanel>();
            }

            if (!DefaultInventoryPanel)
            {
                DefaultInventoryPanel = PanelManager.Instance.GetPanel<InventoryImagePanel>();
            }
        }

        public override void DidShow()
        {
            if (ItemViewContainer)
            {
                ItemViewContainer.HideAll();
            }

            base.DidShow();
        }

        public override void Populate(InventoryTableItem item)
        {
            var collectible = item.InventoryCollectible.Collectible;
            bool didShow = false;

            if (collectible.Attachments != null && collectible.Attachments.Length > 0)
            {
                var content = collectible.Attachments[0];

                if (content is MediaContent)
                {
                    var media = (MediaContent)content;

                    if (media.MediaItem != null)
                    {
                        switch (media.MediaItem.MediaType)
                        {
                            case MediaType.Audio:
                                if (InventoryAudioPanel)
                                {
                                    didShow = true;
                                    PanelManager.Instance.Push(InventoryAudioPanel, item.InventoryCollectible);
                                }
                                break;
                            case MediaType.Video:
                                if (InventoryVideoPanel)
                                {
                                    didShow = true;
                                    PanelManager.Instance.Push(InventoryVideoPanel, item.InventoryCollectible);
                                }
                                break;
                            case MediaType.Image:
                                if (InventoryImagePanel)
                                {
                                    didShow = true;
                                    PanelManager.Instance.Push(InventoryImagePanel, item.InventoryCollectible);
                                }
                                break;
                        }
                    }
                }
                else if (content is ScreenMessage)
                {
                    if (InventoryScreenMessagePanel)
                    {
                        didShow = true;
                        PanelManager.Instance.Push(InventoryScreenMessagePanel, item.InventoryCollectible);
                    }
                }
            }

            if (!didShow)
            {
                if (DefaultInventoryPanel)
                {
                    PanelManager.Instance.Push(DefaultInventoryPanel, item.InventoryCollectible);
                }
            }

            base.Populate(item);
        }
    }

}