// Copyright (c) 2018 RocketChicken Interactive Inc.

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Represents an amount of a currency.
    /// </summary>
    public class CurrencyCount
    {
        public string Currency { get; set; }
        public int Count { get; set; }

        public CurrencyCount()
        {
        }

        public CurrencyCount(string currency, int count)
        {
            Currency = currency;
            Count = count;
        }

        public override string ToString()
        {
            return string.Format("[CurrencyCount: Currency={0}, Count={1}]", Currency, Count);
        }
    }
}