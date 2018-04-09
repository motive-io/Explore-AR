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
    /// Adds or removes currency from a player's wallet. 
    /// </summary>
	public class WalletCurrency : ScriptObject
	{
	    public CurrencyCount[] CurrencyCounts { get; set; }
	}
}