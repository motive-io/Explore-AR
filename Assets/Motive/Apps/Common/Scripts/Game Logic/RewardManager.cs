// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using Motive.UI.Framework;
using System;

namespace Motive.Unity.Gaming
{

    public class RewardEventArgs : EventArgs
    {
        public ValuablesCollection Valuables { get; private set; }

        public RewardEventArgs(ValuablesCollection valuables)
        {
            this.Valuables = valuables;
        }
    }

    /// <summary>
    /// Handles giving player "rewards."
    /// </summary>
    public class RewardManager : Singleton<RewardManager>
    {
        public Panel RewardPanel { get; set; }
        public event EventHandler<RewardEventArgs> RewardAdded;

        public void ActivatePlayerReward(PlayerReward reward)
        {
            if (reward.Reward != null)
            {
                RewardValuables(reward.Reward);
            }
        }

        /// <summary>
        /// Rewards the valuables.
        /// </summary>
        /// <param name="valuables">Valuables.</param>
        /// <param name="onReward">Callback after the valuables have been rewarded to the player.</param>
        public void RewardValuables(ValuablesCollection valuables, Action onReward = null)
        {
            if (RewardAdded != null)
            {
                RewardAdded(this, new RewardEventArgs(valuables));
            }

            // If there's a reward panel, add the valuables to the inventory after
            // the player confirms.
            if (RewardPanel)
            {
                PanelManager.Instance.Push(RewardPanel, valuables, () =>
                {
                    TransactionManager.Instance.AddValuables(valuables);

                    if (onReward != null)
                    {
                        onReward();
                    }
                });
            }
            else
            {
                if (onReward != null)
                {
                    onReward();
                }

                TransactionManager.Instance.AddValuables(valuables);
            }
        }
    }

}