// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Globalization;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Models
{
    public class LocationCategory : ScriptObject
    {
        public LocalizedText LocalizedTitle { get; set; }
        public LocalizedText LocalizedDescription { get; set; }
        public string[] LocationTypes { get; set; }
        public string[] StoryTags { get; set; }
        public LocationCategory[] SubCategories { get; set; }

        public string Title
        {
            get
            {
                return LocalizedText.GetText(LocalizedTitle);
            }
        }

        public string Description
        {
            get
            {
                return LocalizedText.GetText(LocalizedDescription);
            }
        }
    }
}
