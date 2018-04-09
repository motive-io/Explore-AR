// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using Motive.Core.Scripting;
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using System.Collections.Generic;
using System.Linq;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class CanCompleteObjectiveTaskConditionMonitor :
        SynchronousConditionMonitor<CanCompleteObjectiveTaskCondition>
    {
        public CanCompleteObjectiveTaskConditionMonitor()
            : base("motive.gaming.canCompleteObjectiveTaskCondition")
        {
            TaskManager.Instance.Updated += UpdatedEventHandler;
            Inventory.Instance.Updated += UpdatedEventHandler;
            Wallet.Instance.Updated += UpdatedEventHandler;
        }

        void UpdatedEventHandler(object sender, System.EventArgs args)
        {
            CheckWaitingConditions();
        }

        public override bool CheckState(FrameOperationContext fop, CanCompleteObjectiveTaskCondition condition, out object[] results)
        {
            results = null;

            if (condition.ObjectiveReference != null)
            {
                var task = TaskManager.Instance.GetTaskDriverForObjective(condition.ObjectiveReference.ObjectId);

                if (task != null)
                {
                    return TaskManager.Instance.CanComplete(task);
                }
            }

            return false;
        }
    }
}
