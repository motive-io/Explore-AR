using System;
using UnityEngine;

namespace Motive.UI.Framework
{
    public class TableItemSelector : MonoBehaviour
    {
        int m_selectedIdx = -1;
        GameObject m_currHighlighted;

        public Table Table;

        private void Start()
        {
            if (!Table)
            {
                Table = GetComponent<Table>();
            }

            if (Table)
            {
                Table.OnUpdated.AddListener(() => HighlightItem(Math.Max(0, m_selectedIdx)));
            }

            HighlightItem(0);
        }

        public void HighlightItem(int idx)
        {
            m_currHighlighted = null;
            m_selectedIdx = -1;

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);

                if (i == idx)
                {
                    m_selectedIdx = idx;

                    m_currHighlighted = child.gameObject;
                }

                var selectedItem = child.GetComponent<SelectableTableItem>();

                if (selectedItem)
                {
                    selectedItem.SetHighlighted(i == idx);
                }
            }
        }

        public void Next()
        {
            if (transform.childCount > 0)
            {
                var idx = (++m_selectedIdx) % transform.childCount;

                HighlightItem(idx);
            }
        }

        public void Prev()
        {
            if (transform.childCount > 0)
            {
                var idx = (--m_selectedIdx) % transform.childCount;

                HighlightItem(idx);
            }
        }

        public void SelectItem()
        {
            if (m_currHighlighted != null)
            {
                var slct = m_currHighlighted.GetComponent<SelectableTableItem>();

                if (slct) slct.Select();
            }
        }
    }
}