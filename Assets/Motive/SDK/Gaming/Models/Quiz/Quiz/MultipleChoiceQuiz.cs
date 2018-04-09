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
    /// A quiz that takes multiple choice anwers.
    /// </summary>
    public class MultipleChoiceQuiz : QuizBase
    {
        public MultipleChoiceQuizResponse[] Responses { get; set; }

        //public override void GetMediaItems(IList<MediaItem> items)
        //{
        //    if (Responses != null)
        //    {
        //        foreach (var answer in Responses)
        //        {
        //            answer.GetMediaItems(items);
        //        }
        //    }

        //    base.GetMediaItems(items);
        //}
    }
}
