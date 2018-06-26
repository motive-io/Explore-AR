// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using Motive.UI.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Motive.Core.Models;
using Motive.Gaming.Models;
using UnityEngine;
using UnityEngine.UI;
using Motive.Unity.Gaming;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays a set of collectibles in sequence. Good for apps that
    /// want to reward a player with multiple items and show them
    /// details for each one.
    /// </summary>
    public class CollectibleCollectionDetailsPanel : Panel<IEnumerable<Collectible>>
    {
        public InventoryAudioPanel AudioPanel;
        public InventoryImagePanel ImagePanel;
        public InventoryVideoPanel VideoPanel;

        private List<Collectible> m_collectiblesToShow;
        public override void Populate([NotNull] IEnumerable<Collectible> data)
        {
            base.Populate(data);

            m_collectiblesToShow = null;
            m_collectiblesToShow = Data.ToList();
            PushNextCollectibleOrClose();
        }

        public void PushCollectibleDetails(Collectible collectible, Action onClose = null)
        {
            var content = collectible.Content;

            var mediaContent = content as MediaContent;

            var data = new InventoryCollectible(collectible, 1);

            if (mediaContent != null && mediaContent.MediaItem != null)
            {
                switch (mediaContent.MediaItem.MediaType)
                {
                    case Motive.Core.Media.MediaType.Video:
                        PanelManager.Instance.Push(VideoPanel, data, onClose);
                        return;
                    case Motive.Core.Media.MediaType.Audio:
                        PanelManager.Instance.Push(AudioPanel, data, onClose);
                        return;
                    case Motive.Core.Media.MediaType.Image:
                        PanelManager.Instance.Push(ImagePanel, data, onClose);
                        return;
                }
            }

            // Otherwise
            PanelManager.Instance.Push(ImagePanel, data, onClose);
        }

        public void PushNextCollectibleOrClose()
        {
            if (m_collectiblesToShow == null || !m_collectiblesToShow.Any())
            {
                Close();
                return;
            }

            var c = m_collectiblesToShow[0];
            m_collectiblesToShow.RemoveAt(0);

            if (c == null) { Close(); }

            PushCollectibleDetails(c, PushNextCollectibleOrClose);

        }
    }
}