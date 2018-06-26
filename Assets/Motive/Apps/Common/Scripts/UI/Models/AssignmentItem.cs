// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Globalization;
using Motive.Gaming.Models;
using Motive.Unity.Gaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Base class for assignment items.
    /// </summary>
    public abstract class AssignmentItem
    {
        public abstract string Title { get; }
        public abstract string Description { get; }
        public abstract string ImageUrl { get; }
        public abstract string ImageTitle { get; }
        public abstract bool IsComplete { get; }
        public virtual int Index { get; protected set; }
        public AssignmentDriver AssignmentDriver { get; protected set; }

        public abstract object GetData();

        public AssignmentItem(AssignmentDriver asnDriver, int idx = 0)
        {
            AssignmentDriver = asnDriver;
            Index = idx;
        }
    }

    /// <summary>
    /// An assignment item represented by a task.
    /// </summary>
    public class TaskAssignmentItem : AssignmentItem
    {
        public IPlayerTaskDriver TaskDriver { get; protected set; }

        public override string Title
        {
            get
            {
                return TaskDriver.Task.Title;
            }
        }

        public override string Description
        {
            get
            {
                return TaskDriver.Task.Description;
            }
        }

        public override string ImageUrl
        {
            get
            {
                return TaskDriver.Task.ImageUrl;
            }
        }

        public override bool IsComplete
        {
            get
            {
                return TaskDriver.IsComplete;
            }
        }

        public override string ImageTitle
        {
            get
            {
                if (TaskDriver.Task.LocalizedImage != null &&
                    TaskDriver.Task.LocalizedImage.MediaItem != null)
                {
                    return TaskDriver.Task.LocalizedImage.MediaItem.Title;
                }

                return null;
            }
        }

        public TaskAssignmentItem(AssignmentDriver asnDriver, IPlayerTaskDriver driver, int idx = 0)
            : base(asnDriver, idx)
        {
            TaskDriver = driver;
        }

        public override object GetData()
        {
            return TaskDriver;
        }
    }

    /// <summary>
    /// An assignment item represented by an objective.
    /// </summary>
    public class ObjectiveAssignmentItem : AssignmentItem
    {
        public AssignmentObjective AssignmentObjective { get; protected set; }

        public TaskObjective Objective
        {
            get
            {
                return AssignmentObjective.Objective;
            }
        }

        public override string Title
        {
            get
            {
                return Objective.Title;
            }
        }

        public override string Description
        {
            get
            {
                return Objective.Description;
            }
        }

        public override string ImageUrl
        {
            get
            {
                return Objective.ImageUrl;
            }
        }

        public override bool IsComplete
        {
            get
            {
                return TaskManager.Instance.IsObjectiveComplete(Objective.Id);
            }
        }

        public override string ImageTitle
        {
            get
            {
                var mediaItem = LocalizedMedia.GetMediaItem(Objective.LocalizedImage);

                return mediaItem != null ? mediaItem.Title : null;
            }
        }

        public ObjectiveAssignmentItem(AssignmentDriver asnDriver, AssignmentObjective objective, int idx = 0)
            : base(asnDriver, idx)
        {
            AssignmentObjective = objective;
        }

        public override object GetData()
        {
            return Objective;
        }
    }

}