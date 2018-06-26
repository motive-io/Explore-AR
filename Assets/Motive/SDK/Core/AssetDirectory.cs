// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Core
{
    /// <summary>
    /// A directory of ScriptObjects that can be queried by ID.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AssetDirectory<T> where T : ScriptObject
    {
        protected class AssetDirectoryItem
        {
            public T Item;
            public Catalog<T> Catalog;
        }

        protected Dictionary<string, AssetDirectoryItem> m_directory;
        protected Dictionary<string, Catalog<T>> m_catalogs;

        public virtual IEnumerable<T> AllItems
        {
            get
            {
                return m_catalogs.SelectMany(c => c.Value);
            }
        }

        public AssetDirectory()
        {
            m_directory = new Dictionary<string, AssetDirectoryItem>();
            m_catalogs = new Dictionary<string, Catalog<T>>();
        }

        protected virtual void AddItem(T item)
        {
        }

        public virtual void AddCatalog(Catalog<T> catalog)
        {
            m_catalogs[catalog.Name] = catalog;

            foreach (var item in catalog)
            {
                m_directory[item.Id] = new AssetDirectoryItem
                {
                    Item = item,
                    Catalog = catalog
                };

                AddItem(item);
            }
        }

        public virtual Catalog<T> GetCatalogByName(string catName)
        {
            if (m_catalogs.ContainsKey(catName))
            {
                return m_catalogs[catName];
            }

            return null;
        }

        public virtual T GetItem(ObjectReference objRef)
        {
            if (objRef != null && objRef.ObjectId != null)
            {
                return GetItem(objRef.ObjectId);
            }

            return default(T);
        }

        public virtual T GetItem(string objectId)
        {
            if (objectId == null || m_directory == null)
            {
                return default(T);
            }

            AssetDirectoryItem item = null;

            if (m_directory.TryGetValue(objectId, out item))
            {
                return item.Item;
            }

            return default(T);
        }

        public virtual Catalog<T> GetCatalogForItem(string objectId)
        {
            if (objectId == null)
            {
                return null;
            }

            AssetDirectoryItem item = null;

            if (m_directory.TryGetValue(objectId, out item))
            {
                return item.Catalog;
            }

            return null;
        }

        public virtual Catalog<T> GetCatalogForItem(ObjectReference objRef)
        {
            if (objRef != null)
            {
                return GetCatalogForItem(objRef.ObjectId);
            }

            return null;
        }

        public virtual string GetCatalogNameForItem(string objectId)
        {
            var cat = GetCatalogForItem(objectId);

            if (cat != null)
            {
                return cat.Name;
            }

            return null;
        }

        public virtual string GetCatalogNameForItem(ObjectReference objRef)
        {
            var cat = GetCatalogForItem(objRef);

            if (cat != null)
            {
                return cat.Name;
            }

            return null;
        }

        public virtual IEnumerable<T> GetItemsWhere(Func<T, bool> predicate)
        {
            return m_directory.Values.Where(c => predicate(c.Item)).Select(c => c.Item);
        }

        public virtual void Clear()
        {
            m_directory.Clear();
        }
    }
}
