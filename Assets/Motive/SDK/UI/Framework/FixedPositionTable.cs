// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;

namespace Motive.UI.Framework
{
    /// <summary>
    /// A table where the items are laid out in fixed positions.
    /// </summary>
    public class FixedPositionTable : Table
    {
        public GameObject[] ShowWhenOverflow;
        public GameObject[] ShowWhenUndeflow;

        public Transform[] ItemParents;

        public int Offset;

        public override void AttachItem(Transform item, int idx)
        {
            idx = idx - Offset;

            if (idx >= 0 && idx < ItemParents.Length)
            {
                item.SetParent(ItemParents[idx], false);
            }
        }

        public override void RemoveFrom(int idx)
        {
            var start = Mathf.Max(idx - Offset, 0);

            for (int i = start; i < ItemParents.Length; i++)
            {
                if (i < 0) continue;

                var parent = ItemParents[i];
                
                Transform[] children = new Transform[parent.childCount];

                for (int c = 0; c < parent.childCount; c++)
                {
                    var child = parent.GetChild(c);
                    children[c] = child;
                }

                foreach (var child in children)
                {
                    child.SetParent(null);
                    Destroy(child.gameObject);
                }
            }

            if (m_items != null && m_items.Count >= idx)
            {
                m_items.RemoveRange(idx, m_items.Count - idx);
            }
        }
    }
}