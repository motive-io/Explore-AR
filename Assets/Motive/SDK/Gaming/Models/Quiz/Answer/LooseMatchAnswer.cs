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
    /// A free response answer that allows a certain number of incorrect characters
    /// in the answer.
    /// </summary>
    public class LooseMatchAnswer: FreeResponseAnswerBase 
    {
        public int BadCharactersAllowed { get; set; }
    }
}
