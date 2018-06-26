// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class WalletCurrencyLimitProcessor : ThreadSafeScriptResourceProcessor<WalletCurrencyLimit>
    {
        public override void ActivateResource(ResourceActivationContext context, WalletCurrencyLimit resource)
        {
            if (context.IsFirstActivation)
            {
                Wallet.Instance.SetLimits(resource);
            }
        }
    }
}