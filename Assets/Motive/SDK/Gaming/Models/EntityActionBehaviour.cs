// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// An action that an entity can take or absorb, along with the
    /// options required to take the action.
    /// </summary>
    public class EntityActionBehaviour
    {
        public string Action { get; set; }
        public EntityActionActivationOption[] ActivationOptions { get; set; }

        public int GetCurrencyCost(string currency)
        {
            if (ActivationOptions != null)
            {
                var opt = ActivationOptions.Where(
                    o => o.Cost.CurrencyCounts != null &&
                    o.Cost.CurrencyCounts.Any(cc => cc.Currency == currency)).FirstOrDefault();

                if (opt != null)
                {
                    return opt.Cost.GetCurrencyCount(currency);
                }
            }

            return 0;
        }
    }
}
