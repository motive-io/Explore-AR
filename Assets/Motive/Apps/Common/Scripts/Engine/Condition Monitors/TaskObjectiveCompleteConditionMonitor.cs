// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Unity.Gaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Scripting
{
    public class TaskObjectiveCompleteConditionMonitor : SynchronousConditionMonitor<TaskObjectiveCompleteCondition>
    {
        public TaskObjectiveCompleteConditionMonitor() : base("motive.gaming.taskObjectiveCompleteCondition")
        {
            TaskManager.Instance.ObjectivesUpdated += Instance_ObjectivesUpdated;
        }

        void Instance_ObjectivesUpdated(object sender, EventArgs e)
        {
            CheckWaitingConditions();
        }

        public override bool CheckState(FrameOperationContext fop, TaskObjectiveCompleteCondition condition, out object[] results)
        {
            results = null;

            if (condition.TaskObjectiveReference != null)
            {
                return TaskManager.Instance.CheckObjective(condition.TaskObjectiveReference.ObjectId);
            }

            return false;
        }
    }

}