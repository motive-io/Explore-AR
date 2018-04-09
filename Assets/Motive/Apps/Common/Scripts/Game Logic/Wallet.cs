// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive;
using Motive.Core.Json;
using Motive.Core.Storage;
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using Motive.Unity.Scripting;
using Motive.Unity.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Motive.Unity.Gaming
{
    public class WalletUpdateEventArgs : EventArgs
    {
        public CurrencyCount[] CurrencyCounts { get; private set; }

        public WalletUpdateEventArgs(CurrencyCount[] currencyCounts)
        {
            this.CurrencyCounts = currencyCounts;
        }
    }

    /// <summary>
    /// Tracks a set of key/count pairs for currencies.
    /// </summary>
    public class Wallet : Singleton<Wallet>
    {
        public event EventHandler<WalletUpdateEventArgs> Updated;

        class WalletCurrencyItem
        {
            public string CurrencyName { get; set; }
            public int Count { get; set; }
            public int Limit { get; set; }
        }

        class WalletCurrencyState
        {
            public WalletCurrencyItem[] Currencies { get; set; }
        }

        Dictionary<string, WalletCurrencyItem> m_itemDict;

        string m_stateFile;
        private IStorageAgent m_fileAgent;

        public Wallet()
        {
            m_stateFile = StorageManager.GetGameFileName("wallet.json");
            m_fileAgent = (Platform.Instance.UseEncryption) ? new EncryptedFileStorageManager(Application.persistentDataPath).GetAgent(m_stateFile)
                            : new FileStorageManager(Application.persistentDataPath).GetAgent(m_stateFile);

            m_itemDict = new Dictionary<string, WalletCurrencyItem>();

            var state = JsonHelper.Deserialize<WalletCurrencyState>(m_fileAgent);

            if (state != null && state.Currencies != null)
            {
                foreach (var wis in state.Currencies)
                {
                    m_itemDict[wis.CurrencyName] = wis;
                }
            }

            ScriptManager.Instance.ScriptsReset += Instance_ScriptsReset;
        }

        private void Instance_ScriptsReset(object sender, System.EventArgs e)
        {
            m_itemDict.Clear();

            if (Updated != null)
            {
                Updated(this, new WalletUpdateEventArgs(null));
            }
        }

        internal void SetLimits(WalletCurrencyLimit limits)
        {
            if (limits.CurrencyCounts != null)
            {
                foreach (var limit in limits.CurrencyCounts)
                {
                    WalletCurrencyItem item;

                    if (!m_itemDict.TryGetValue(limit.Currency, out item))
                    {
                        item = new WalletCurrencyItem
                        {
                            CurrencyName = limit.Currency,
                            Limit = int.MaxValue
                        };

                        m_itemDict[item.CurrencyName] = item;
                    }

                    item.Limit = MathHelper.ApplyNumericalOperator(limits.Operator, item.Limit, limit.Count);
                }
            }
        }

        public void Remove(IEnumerable<CurrencyCount> currencyCounts)
        {
            if (currencyCounts != null)
            {
                foreach (var cc in currencyCounts)
                {
                    Add(cc.Currency, -cc.Count, false);
                }

                Save();
            }
        }

        public void Add(IEnumerable<CurrencyCount> currencyCounts)
        {
            if (currencyCounts != null)
            {
                foreach (var cc in currencyCounts)
                {
                    Add(cc.Currency, cc.Count, false);
                }

                Save();
            }
        }

        internal void Add(string currency, int count, bool commit = true)
        {
            WalletCurrencyItem item;

            if (!m_itemDict.TryGetValue(currency, out item))
            {
                item = new WalletCurrencyItem
                {
                    CurrencyName = currency,
                    Limit = int.MaxValue
                };

                m_itemDict[item.CurrencyName] = item;
            }

            var newCount = Math.Min(item.Count + count, item.Limit);

            item.Count = newCount;

            if (commit)
            {
                Save();

                if (Updated != null)
                {
                    Updated(this, new WalletUpdateEventArgs(new CurrencyCount[] { new CurrencyCount { Currency = currency, Count = count } }));
                }
            }
        }

        public void Add(CurrencyCount currencyCount, bool commit = true)
        {
            if (currencyCount == null)
            {
                return;
            }

            Add(currencyCount.Currency, currencyCount.Count, commit);
        }

        void Save()
        {
            var mis = m_itemDict.Values.ToArray();

            JsonHelper.Serialize(m_fileAgent, new WalletCurrencyState { Currencies = mis });
        }

        public void Add(CurrencyCount[] currencyCounts)
        {
            if (currencyCounts != null)
            {
                foreach (var cc in currencyCounts)
                {
                    Add(cc, false);
                }
            }

            Save();

            if (Updated != null)
            {
                Updated(this, new WalletUpdateEventArgs(currencyCounts));
            }
        }

        internal int GetCount(string currency)
        {
            if (m_itemDict.ContainsKey(currency))
            {
                return m_itemDict[currency].Count;
            }

            return 0;
        }
    }

}