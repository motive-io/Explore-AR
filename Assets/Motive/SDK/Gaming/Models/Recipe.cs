// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Globalization;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Used by the crafting system to define the inputs and outputs for
    /// crafting items.
    /// </summary>
    public class Recipe : ScriptObject
    {
        public LocalizedText LocalizedTitle { get; set; }

        public string Title
        {
            get
            {
                return LocalizedText.GetText(LocalizedTitle);
            }
        }

        public ValuablesCollection Requirements { get; set; }

        public ValuablesCollection Ingredients { get; set; }

        public ValuablesCollection Output { get; set; }
    }
}
