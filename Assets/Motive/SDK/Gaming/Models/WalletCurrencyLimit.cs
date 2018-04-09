// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Set a limit on a particular currency in the player's wallet.
    /// </summary>
	public class WalletCurrencyLimit : ScriptObject 
    {
		public NumericalOperator Operator { get; set; }
		public CurrencyCount[] CurrencyCounts { get; set; }
	}
}
