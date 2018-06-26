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
    /// Base class for quiz minigames.
    /// </summary>
    public class QuizBase : ScriptObject, IMediaItemProvider, ITaskMinigame
    {
        //public LocalizedMedia QuestionMedia { get; set; }
        public LocalizedMedia QuestionImage { get; set; }
        public int MaximumAttempts { get; set; }
        public LocalizedText Question { get; set; }

        public string ImageUrl
        {
            get
            {
                return LocalizedMedia.GetMediaUrl(QuestionImage);
            }
        }

        public MediaItem MediaItem
        {
            get
            {
                return LocalizedMedia.GetMediaItem(QuestionImage);
            }
        }

        public virtual void GetMediaItems(IList<Core.Media.MediaItem> items)
        {
            LocalizedMedia.GetMediaItems(QuestionImage, items);
        }
    }
}
