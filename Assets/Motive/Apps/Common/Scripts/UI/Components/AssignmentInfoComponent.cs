// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays assignment information.
    /// </summary>
    public class AssignmentInfoComponent : PanelComponent<AssignmentDriver>
    {
        public Text Title;

        public override void Populate(AssignmentDriver obj)
        {
            SetText(Title, obj.Assignment.Title);

            base.Populate(obj);
        }

        public void CompleteAssignment()
        {
            if (Data != null)
            {
                Data.Close();
            }
        }
    }

}