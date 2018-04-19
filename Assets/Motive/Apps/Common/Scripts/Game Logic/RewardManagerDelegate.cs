using System;
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.Unity.Utilities;
using UnityEngine;

namespace Motive.Unity.Gaming
{
    public class RewardManagerDelegate : SingletonComponent<RewardManagerDelegate>,
        IRewardManagerDelegate
    {
        public Panel RewardPanel;
        public bool ApplyRewardOnPanelClose = true;

        protected override void Awake()
        {
            base.Awake();

            RewardManager.Instance.Delegate = this;
        }

        public void ProcessReward(ValuablesCollection valuables, Action onReward)
        {
            // If there's a reward panel, add the valuables to the inventory after
            // the player confirms.
            if (RewardPanel)
            {
                if (ApplyRewardOnPanelClose)
                {
                    PanelManager.Instance.Push(RewardPanel, valuables, onReward);
                }
                else
                {
                    PanelManager.Instance.Push(RewardPanel, valuables, null);

                    if (onReward != null)
                    {
                        onReward();
                    }
                }
            }
            else
            {
                if (onReward != null)
                {
                    onReward();
                }
            }
        }
    }
}