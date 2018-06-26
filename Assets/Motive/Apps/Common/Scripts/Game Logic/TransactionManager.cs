// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Utilities;
using Motive.Gaming.Models;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Manages transactions of collectibles. Verifies that player has items before
    /// removing from inventory/wallet.
    /// </summary>
    public class TransactionManager : Singleton<TransactionManager>
    {
        public bool Exchange(ValuablesCollection inputs, ValuablesCollection outputs, bool allowDebt = false)
        {
            if (!HasValuables(inputs) && !allowDebt)
            {
                return false;
            }

            RemoveValuables(inputs);
            AddValuables(outputs);

            return true;
        }

        public void RemoveValuables(ValuablesCollection valuables)
        {
            if (valuables != null)
            {
                Inventory.Instance.Remove(valuables.CollectibleCounts);
                Wallet.Instance.Remove(valuables.CurrencyCounts);
            }
        }

        public void AddValuables(ValuablesCollection valuables)
        {
            if (valuables != null)
            {
                Inventory.Instance.Add(valuables.CollectibleCounts);
                Wallet.Instance.Add(valuables.CurrencyCounts);
            }
        }

        public bool HasValuables(ValuablesCollection valuables)
        {
            if (valuables == null)
            {
                return true;
            }

            if (valuables.CollectibleCounts != null)
            {
                foreach (var cc in valuables.CollectibleCounts)
                {
                    var ct = Inventory.Instance.GetCount(cc.CollectibleId);

                    if (ct < cc.Count)
                    {
                        return false;
                    }
                }
            }

            if (valuables.CurrencyCounts != null)
            {
                foreach (var cc in valuables.CurrencyCounts)
                {
                    var ct = Wallet.Instance.GetCount(cc.Currency);

                    if (ct < cc.Count)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

}