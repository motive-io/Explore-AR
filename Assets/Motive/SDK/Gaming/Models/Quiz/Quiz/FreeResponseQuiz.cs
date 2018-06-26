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
    /// A quiz that takes a freeform response.
    /// </summary>
    public class FreeResponseQuiz : QuizBase
    {
        public FreeResponseAnswerBase Answer { get; set; }
    }
}
