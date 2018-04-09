// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Motive.UI.Framework
{
    /// <summary>
    /// Used to display a collection of items, usually in a layout group.
    /// </summary>
    public class Table : MonoBehaviour
    {
       protected List<GameObject> m_items;

        public IEnumerable<GameObject> Items { get { return m_items; } }

        void Awake()
        {
            m_items = new List<GameObject>();
        }

        public IEnumerable<T> GetItems<T>() where T : Component
        {
            return m_items.Select(i => i.GetComponent<T>())
                .Where(i => i != null)
                .ToList();
        }

        public virtual void RemoveFrom(int idx)
        {
            var count = transform.childCount - idx;

            if (count < 0)
            {
                return;
            }

            Transform[] children = new Transform[count];

            for (int i = idx; i < count; i++)
            {
                var child = transform.GetChild(i);
                children[i] = child;
            }

            foreach (var child in children)
            {
                child.SetParent(null);
                Destroy(child.gameObject);
            }

            if (m_items != null && count > 0)
            {
                m_items.RemoveRange(idx, Math.Min(m_items.Count, count));
            }
        }

        public virtual void Clear()
        {
            RemoveFrom(0);
        }

		public GameObject AddItem(GameObject prefab)
		{
			var obj = Instantiate(prefab);

            m_items.Add(obj);

            AttachItem(obj.transform, m_items.Count - 1);

			return obj;
		}

        public virtual void AttachItem(Transform item, int idx)
        {
            item.SetParent(transform, false);
        }

        public T AddItem<T>(T prefab) where T : Component
        {
            var obj = Instantiate<T>(prefab);

            m_items.Add(obj.gameObject);

            AttachItem(obj.gameObject.transform, m_items.Count - 1);

			return obj;
        }

        public void RemoveItem<T>(T item) where T : Component
        {
            if (item)
            {
                m_items.Remove(item.gameObject);

                item.transform.SetParent(null);
                Destroy(item.gameObject);
            }
        }

        /// <summary>
        /// Adds a table item at an index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prefab"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public T AddItemAtIndex<T>(T prefab, int index) where T : Component
        {
            var obj = Instantiate<T>(prefab);
            obj.transform.SetParent(transform, false);
            obj.transform.SetSiblingIndex(index);

            m_items.Add(obj.gameObject);

            return obj;
        }
    }
}