// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Globalization;
using Motive.Core.Media;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Base class for response quiz answer types.
    /// </summary>
    public class FreeResponseAnswerBase : ScriptObject
    {
        public LocalizedText[] AnswerText { get; set; }
        public bool EnforceCase { get; set; }

    }
}
