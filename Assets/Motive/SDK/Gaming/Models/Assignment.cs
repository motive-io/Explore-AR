// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Linq;
using System.Collections.Generic;
using Motive.Core.Json;
using Motive.Core.Scripting;
using Motive.Core.Globalization;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// An assignment defines a set of "objectives" to be met by the player.
    /// Objectives can be met by completing tasks or completed directly
    /// using the Motive ObjectiveCompleter resource.
    /// </summary>
    public class Assignment : ScriptResource, IMediaItemProvider
    {
        public LocalizedText LocalizedTitle { get; set; }
        public LocalizedText LocalizedDescription { get; set; }
        public ValuablesCollection Reward { get; set; }
        public AssignmentObjective[] Objectives { get; set; }

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

        public void GetMediaItems(IList<Core.Media.MediaItem> items)
        {
            if (Objectives != null)
            {
                foreach (var o in Objectives)
                {
                    if (o.Objective != null)
                    {
                        o.Objective.GetMediaItems(items);
                    }
                }
            }
        }
    }
}