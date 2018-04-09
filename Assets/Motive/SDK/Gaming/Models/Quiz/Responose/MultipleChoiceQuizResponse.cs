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
    /// Multiple choice quiz response.
    /// </summary>
    public class MultipleChoiceQuizResponse: QuizResponseBase
    {
        /// <summary>
        /// If true this is one of the correct answers.
        /// </summary>
        public bool IsCorrect { get; set; }
    }
}
