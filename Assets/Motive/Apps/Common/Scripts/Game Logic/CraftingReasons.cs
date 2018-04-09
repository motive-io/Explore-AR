// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.ComponentModel;
using System.Reflection;

namespace Motive.Unity.Gaming
{
    public enum CraftingReason
    {
        [Description("REQUIREMENTS MET")]
        RequirementsMet,

        [Description("REQUIREMENTS NOT MET")]
        RequirementsUnmet,

        [Description("NEED MORE CURRENCY")]
        XpUnmet,

        [Description("NO REQUIREMENTS")]
        NoRequirements,

        [Description("NO RECIPE")]
        NoRecipe
    }
}