// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using Motive.UI.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Represents an annotation for an AR object displayed
    /// on the AR canvas.
    /// </summary>
    public class ARAnnotationContainer : MonoBehaviour
    {
        public Vector2 MinSize = new Vector2(70, 70);

        public Table InteractibleTable;
        public Table CollectibleTable;

        public ARAnnotationItem CollectibleItem;
        public ARAnnotationItem TapItem;

        public Dictionary<string, ARAnnotationItem> m_collectibleItems;

        private ARAnnotationItem m_tapItem;

        void Awake()
        {
            InteractibleTable.Clear();
            CollectibleTable.Clear();

            m_collectibleItems = new Dictionary<string, ARAnnotationItem>();
        }

        public void AddTapAnnotation(string text, Action onClick = null)
        {
            if (!m_tapItem)
            {
                m_tapItem = InteractibleTable.AddItem(TapItem);
            }

            m_tapItem.SetText(text);
        }

        public void AddCollectibleAnnotation(Collectible collectible, Action onClick = null)
        {
            if (!m_collectibleItems.ContainsKey(collectible.Id))
            {
                var item = CollectibleTable.AddItem(CollectibleItem);

                item.SetImage(collectible.ImageUrl);

                item.OnSelected.AddListener(() =>
                    {
                        if (onClick != null)
                        {
                            onClick();
                        }
                    });

                m_collectibleItems[collectible.Id] = item;
            }
        }

        internal void RemoveCollectibleAnnotation(Collectible collectible)
        {
            ARAnnotationItem item;

            if (m_collectibleItems.TryGetValue(collectible.Id, out item))
            {
                m_collectibleItems.Remove(collectible.Id);

                CollectibleTable.RemoveItem(item);
            }
        }

        public bool HasAnnotations
        {
            get
            {
                return CollectibleTable.Items.Count() + InteractibleTable.Items.Count() > 0;
            }
        }

        internal void RemoveTapAnnotation()
        {
            if (m_tapItem)
            {
                InteractibleTable.RemoveItem(m_tapItem);
                m_tapItem = null;
            }
        }
    }

}