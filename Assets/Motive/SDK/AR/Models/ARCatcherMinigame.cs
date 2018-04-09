// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Unity.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Models
{
    /// <summary>
    /// A task minigame using a simple AR "catcher" mechanic.
    /// </summary>
    public class ARCatcherMinigame : ScriptObject, ITaskMinigame
    {
        public bool HideMapAnnotation { get; set; }

		public LocationAugmentedOptions AROptions { get; set; }
    }
}
