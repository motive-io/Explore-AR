using Motive.UI.Framework;
using Motive.Unity.Apps;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Sets a todo override if a user selects an objective assignment item.
    /// </summary>
    public class ObjectiveAssignmentItemTodoSetterComponent : PanelComponent<ObjectiveAssignmentItem>
    {
        public override void Populate(ObjectiveAssignmentItem obj)
        {
            TodoManager.Instance.OverrideTodoObjective(obj.Objective);

            Back();
        }
    }
}