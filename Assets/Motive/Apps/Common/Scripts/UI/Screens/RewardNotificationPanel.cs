// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Motive.Unity.UI
{
    class RewardNotificationPanel : RewardPanel
    {
        public float SpawnEvery = 1;
        public float FadeDuration = 1;
        public float ShowDuration = 6;

        private List<FadeDestroyInventoryTableItem> m_toStart;
        private float m_sinceLastSpawn;
        public override void Populate(ValuablesCollection data)
        {
            base.Populate(data);

            m_toStart = CollectiblesTable.GetItems<FadeDestroyInventoryTableItem>().ToList();
            m_sinceLastSpawn = float.MaxValue;
        }

        void Update()
        {
            if (!CollectiblesTable.Items.Any())
            {
                Back();
            }

            if (m_sinceLastSpawn >= SpawnEvery && m_toStart.Any())
            {
                var item = m_toStart.FirstOrDefault();
                m_toStart.RemoveAt(0);

                item.FadeDuration = this.FadeDuration;
                item.ShowDuration = this.ShowDuration;

                if (item != null)
                {


                    var _item = item;
                    item.StartAnimation(() => { CollectiblesTable.RemoveItem(_item); });
                }

                m_sinceLastSpawn = 0;
                return;
            }

            m_sinceLastSpawn += Time.deltaTime;
        }
    }

}