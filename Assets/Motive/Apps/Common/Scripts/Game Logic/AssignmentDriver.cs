// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Unity.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Manages the lifecycle of an assignment.
    /// </summary>
    public class AssignmentDriver
    {
        public Assignment Assignment { get; set; }
        public ResourceActivationContext ActivationContext { get; set; }

        public bool IsComplete
        {
            get
            {
                return ActivationContext.CheckEvent("complete");
            }
        }

        public int TotalObjectiveCount
        {
            get
            {
                var objectives = TaskManager.Instance.GetActiveObjectivesForAssignment(Assignment);

                if (objectives != null)
                {
                    return objectives.Count();
                }

                return 0;
            }
        }

        public int CompletedObjectiveCount
        {
            get
            {
                int ct = 0;
                var objectives = TaskManager.Instance.GetActiveObjectivesForAssignment(Assignment);

                if (objectives != null)
                {
                    foreach (var obj in objectives)
                    {
                        if (obj.Objective != null)
                        {
                            if (TaskManager.Instance.IsObjectiveComplete(obj.Objective.Id))
                            {
                                ct++;
                            }
                        }
                    }
                }

                return ct;
            }
        }

        public AssignmentDriver(ResourceActivationContext activationContext, Assignment assignment)
        {
            this.ActivationContext = activationContext;
            this.Assignment = assignment;
        }

        public virtual void Close()
        {
            TaskManager.Instance.CloseAssignment(this);
        }

        protected virtual void Complete()
        {
            TaskManager.Instance.CompleteAssignment(this);

            AnalyticsHelper.FireEvent("CompleteAssignment", new Dictionary<string, object>()
            {
                { "Id", Assignment.Id },
                { "Title", Assignment.Title }
            });
        }

        public virtual void CheckComplete()
        {
            bool isComplete = true;

            if (Assignment.Objectives != null)
            {
                foreach (var objective in Assignment.Objectives)
                {
                    if (objective.Objective != null && !objective.IsOptional)
                    {
                        isComplete &= TaskManager.Instance.IsObjectiveComplete(objective.Objective.Id);
                    }
                }
            }

            if (isComplete)
            {
                // fire close & complete
                Complete();
            }
        }
    }

}