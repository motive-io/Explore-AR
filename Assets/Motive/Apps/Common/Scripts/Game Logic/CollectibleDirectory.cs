// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core;
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using System.Collections.Generic;
using System.Linq;

namespace Motive.Unity.Gaming
{
    public class CollectibleSet
    {
        public Collectible Collectible { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Collectibles indexed by ID, attributes, and emitted and received actions.
    /// </summary>
    public class CollectibleDirectory : SingletonAssetDirectory<CollectibleDirectory, Collectible>
    {
        protected SetDictionary<string, Collectible> m_itemsByAttr;
        protected SetDictionary<string, Collectible> m_itemsByEmitAction;
        protected SetDictionary<string, Collectible> m_itemsByReceivedAction;

        public CollectibleDirectory() : base()
        {
            m_itemsByAttr = new SetDictionary<string, Collectible>();
            m_itemsByEmitAction = new SetDictionary<string, Collectible>();
            m_itemsByReceivedAction = new SetDictionary<string, Collectible>();
        }

        /// <summary>
        /// Sort Items into SetDictionaries to query faster.
        /// </summary>
        /// <param name="item"></param>
        protected override void AddItem(Collectible item)
        {
            base.AddItem(item);

            if (item == null) return;

            // attributes
            if (item.Attributes != null && item.Attributes.Count() > 0)
            {
                foreach (var attr in item.Attributes)
                {
                    m_itemsByAttr.Add(attr, item);
                }
            }

            // emits actions
            if (item.EmittedActions != null && item.EmittedActions.Count() > 0)
            {
                foreach (var act in item.EmittedActions)
                {
                    if (!string.IsNullOrEmpty(act.Action))
                    {
                        m_itemsByEmitAction.Add(act.Action, item);
                    }
                }
            }

            // received
            if (item.ReceivedActions != null && item.ReceivedActions.Count() > 0)
            {
                foreach (var act in item.ReceivedActions)
                {
                    if (!string.IsNullOrEmpty(act.Action))
                    {
                        m_itemsByReceivedAction.Add(act.Action, item);
                    }
                }
            }
        }

        public IEnumerable<Collectible> GetCollectiblesWithAttribute(IEnumerable<string> attrList)
        {
            if (AllItems == null || AllItems.Count() <= 0)
            {
                return null;
            }

            return this.m_itemsByAttr.Where(kvp => attrList.Contains(kvp.Key)).SelectMany(kvp => kvp.Value);
        }


        public IEnumerable<CollectibleSet> GetCollectibleSets(IEnumerable<CollectibleCount> counts)
        {
            if (counts == null)
            {
                return new CollectibleSet[0];
            }

            List<CollectibleSet> sets = new List<CollectibleSet>();

            foreach (var cc in counts)
            {
                var collectible = GetItem(cc.CollectibleId);

                if (collectible != null)
                {
                    sets.Add(new CollectibleSet { Collectible = collectible, Count = cc.Count });
                }
            }

            return sets;
        }

        public Collectible GetFirstCollectible(ValuablesCollection valuables)
        {
            if (valuables == null || valuables.CollectibleCounts == null || valuables.CollectibleCounts.Length == 0)
            {
                return null;
            }

            return GetItem(valuables.CollectibleCounts[0].CollectibleId);
        }

        public IEnumerable<CollectibleSet> GetCollectibleSets(ValuablesCollection valuables)
        {
            if (valuables == null || valuables.CollectibleCounts == null)
            {
                return new CollectibleSet[0];
            }

            return GetCollectibleSets(valuables.CollectibleCounts);
        }

        public IEnumerable<Collectible> GetCollectiblesEmitAction(string action)
        {
            if (AllItems == null || AllItems.Count() <= 0)
            {
                return null;
            }

            return this.m_itemsByEmitAction.Where(kvp => kvp.Key.ToLower() == action.ToLower()).SelectMany(kvp => kvp.Value);
        }

        public IEnumerable<Collectible> GetCollectiblesReceiveAction(string action)
        {
            if (AllItems == null || AllItems.Count() <= 0)
            {
                return null;
            }

            return this.m_itemsByReceivedAction.Where(kvp => kvp.Key.ToLower() == action.ToLower()).SelectMany(kvp => kvp.Value);
        }

        public IEnumerable<Collectible> GetItemsForActionOnEntity(IActiveEntity actee, string action)
        {
            if (AllItems == null || AllItems.Count() <= 0)
            {
                return null;
            }

            return AllItems.Where(c =>
                    c != null &&
                    ActiveEntityManager.Instance.CanActOn(c, actee, action)).ToList();
        }
    }

}