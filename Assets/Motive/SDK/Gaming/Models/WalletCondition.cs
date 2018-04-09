// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// A condition that can monitor the currencies held in the player's wallet.
    /// </summary>
	public class WalletCondition : AtomicCondition
	{
	    public NumericalConditionOperator Operator { get; set; }
	    public CurrencyCount CurrencyCount { get; set; }
	}
}