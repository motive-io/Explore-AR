// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using Motive.Unity.Gaming;
using System;

namespace Motive.Unity.Scripting
{
    public class WalletConditionMonitor : SynchronousConditionMonitor<WalletCondition>
	{
		public WalletConditionMonitor() : base("motive.gaming.walletCondition")
		{
			Wallet.Instance.Updated += Instance_Updated;
		}

		private void Instance_Updated(object sender, EventArgs e)
		{
			CheckWaitingConditions();
		}

		public override bool CheckState(FrameOperationContext fop, WalletCondition condition, out object[] results)
		{
			results = null;

			if (condition.CurrencyCount == null)
			{
				return false;
			}

			int count = Wallet.Instance.GetCount(condition.CurrencyCount.Currency);

			return MathHelper.CheckNumericalCondition(condition.Operator, count, condition.CurrencyCount.Count);
		}
	}
}