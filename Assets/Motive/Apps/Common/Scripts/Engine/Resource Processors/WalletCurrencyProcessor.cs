// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class WalletCurrencyProcessor : ThreadSafeScriptResourceProcessor<WalletCurrency>
	{
		public override void ActivateResource(ResourceActivationContext context, WalletCurrency resource)
		{
			if (context.IsFirstActivation)
			{
				Wallet.Instance.Add(resource.CurrencyCounts);
			}
		}
	}
}
