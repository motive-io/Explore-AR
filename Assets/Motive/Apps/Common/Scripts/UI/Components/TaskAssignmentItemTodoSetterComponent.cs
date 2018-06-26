// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Apps;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Sets a task as a todo override if selected by a user from an assignment panel.
    /// </summary>
    public class TaskAssignmentItemTodoSetterComponent : PanelComponent<TaskAssignmentItem>
    {
        public override void Populate(TaskAssignmentItem obj)
        {
            TodoManager.Instance.OverrideTodoTask(obj.TaskDriver);

            Back();
        }
    }
}