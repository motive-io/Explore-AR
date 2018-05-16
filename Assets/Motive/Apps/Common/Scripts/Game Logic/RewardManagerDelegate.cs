using System;
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.Unity.Utilities;
using UnityEngine;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Delegate that displays a Rewards Panel when a Reward is given to the player.
    /// </summary>
    public class RewardManagerDelegate : SingletonComponent<RewardManagerDelegate>,
        IRewardManagerDelegate
    {
        public PanelLink RewardPanel;
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
                    RewardPanel.Push(valuables, onReward);
                }
                else
                {
                    RewardPanel.Push(valuables);

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