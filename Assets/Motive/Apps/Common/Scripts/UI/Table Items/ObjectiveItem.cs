// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using Motive.Unity.Gaming;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays objective information in a table.
    /// </summary>
    public class ObjectiveItem : TextImageItem
    {
        public Text ItemNumber;

        public UnityEvent OnObjectiveComplete;
        public UnityEvent OnObjectiveNotComplete;

        public virtual void Populate(AssignmentObjective objective)
        {
            SetText(objective.Objective.Title);
            SetImage(objective.Objective.ImageUrl);

            if (TaskManager.Instance.IsObjectiveComplete(objective.Objective.Id))
            {
                if (OnObjectiveComplete != null)
                {
                    OnObjectiveComplete.Invoke();
                }
            }
            else
            {
                if (OnObjectiveNotComplete != null)
                {
                    OnObjectiveNotComplete.Invoke();
                }
            }
        }
    }

}