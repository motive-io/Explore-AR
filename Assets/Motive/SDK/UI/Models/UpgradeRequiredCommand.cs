// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Globalization;
using Motive.Core.Scripting;

namespace Motive.UI.Models
{
    /// <summary>
    /// Pushes an "update required" message to the screen.
    /// </summary>
    public class UpgradeRequiredCommand : ScriptObject
    {
        public LocalizedText LocalizedMessage { get; set; }
        public LocalizedText LocalizedUpgradeButtonText { get; set; }

        public string AndroidStoreURL { get; set; }
        public string IOSStoreURL { get; set; }
    }
}
