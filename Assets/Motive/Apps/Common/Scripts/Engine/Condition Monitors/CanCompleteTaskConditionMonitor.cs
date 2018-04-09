// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Scripting;
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using System.Collections.Generic;
using System.Linq;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class CanCompleteTaskConditionMonitor :
        SynchronousConditionMonitor<CanCompleteTaskCondition>
    {
        public CanCompleteTaskConditionMonitor()
            : base("motive.gaming.canCompleteTaskCondition")
        {
            TaskManager.Instance.Updated += UpdatedEventHandler;
            Inventory.Instance.Updated += UpdatedEventHandler;
            Wallet.Instance.Updated += UpdatedEventHandler;
        }

        void UpdatedEventHandler(object sender, System.EventArgs args)
        {
            CheckWaitingConditions();
        }

        public override bool CheckState(FrameOperationContext fop, CanCompleteTaskCondition condition, out object[] results)
        {
            results = null;

            if (condition.TaskReference != null)
            {
                var instanceId = fop.GetInstanceId(condition.TaskReference.ObjectId);

                var driver = TaskManager.Instance.GetDriver(instanceId);

                return TaskManager.Instance.CanComplete(driver);
            }

            return false;
        }
    }
    
}