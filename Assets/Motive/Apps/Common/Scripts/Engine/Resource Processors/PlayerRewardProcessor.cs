// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class PlayerRewardProcessor : ThreadSafeScriptResourceProcessor<PlayerReward>
    {
        public override void ActivateResource(ResourceActivationContext context, PlayerReward resource)
        {
            if (!context.IsClosed)
            {
                RewardManager.Instance.ActivatePlayerReward(resource);
                context.Close();
            }
        }
    }
}