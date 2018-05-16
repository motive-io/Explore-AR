// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using Motive.UI.Framework;
using System;

namespace Motive.Unity.Gaming
{
    public interface IRewardManagerDelegate
    {
        void ProcessReward(ValuablesCollection valuables, Action onReward);
    }

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
        public IRewardManagerDelegate Delegate { get; set; }
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

            Action doReward = () =>
            {
                if (onReward != null)
                {
                    onReward();
                }

                TransactionManager.Instance.AddValuables(valuables);
            };

            if (Delegate != null)
            {
                Delegate.ProcessReward(valuables, doReward);
            }
            else
            {
                doReward();
            }
        }
        
        /// <summary>
        /// Show the rewards screen without actually awarding the valuables.
        /// </summary>
        /// <param name="valuables"></param>
        public void ShowRewards(ValuablesCollection valuables, Action onClose = null)
        {
            if (Delegate != null)
            {
                Delegate.ProcessReward(valuables, onClose);
            }
            else
            {
                if (onClose != null)
                {
                    onClose();
                }
            }
        }
    }

}