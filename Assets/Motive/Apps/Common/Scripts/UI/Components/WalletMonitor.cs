// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Apps;
using Motive.Unity.Gaming;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Monitors a player's wallet for a particular currency and updates
    /// a text element with the count.
    /// </summary>
    public class WalletMonitor : MonoBehaviour
    {
        public string Currency;
        public Text Text;
        public int DecimalPlaces;

        bool m_updated;

        void Awake()
        {
            AppManager.Instance.OnLoadComplete(() =>
            {
                Wallet.Instance.Updated += Instance_Updated;

                UpdateText();
            });
        }

        private void Instance_Updated(object sender, WalletUpdateEventArgs e)
        {
            m_updated = true;
        }

        void Update()
        {
            if (m_updated)
            {
                UpdateText();
            }
        }

        void UpdateText()
        {
            double val = Wallet.Instance.GetCount(Currency);

            if (DecimalPlaces > 0)
            {
                val = val / (Math.Pow(10, DecimalPlaces));
            }

            Text.text = val.ToString("N" + DecimalPlaces);
        }
    }

}