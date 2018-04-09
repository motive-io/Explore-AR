// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays information for an assignment item.
    /// </summary>
    public class AssignmentItemInfo : TextImageItem
    {
        public Text Description;

        public Text ItemNumber;
        public Text ImageTitle;

        public UnityEvent OnItemComplete;
        public UnityEvent OnItemNotComplete;

        public GameObject[] ShowWhenComplete;
        public GameObject[] ShowWhenNotComplete;

        public virtual void Populate(AssignmentItem assnItem)
        {
            SetText(assnItem.Title);
            SetImage(assnItem.ImageUrl);
            SetText(ItemNumber, assnItem.Index.ToString());
            SetText(ImageTitle, assnItem.ImageTitle);
            SetText(Description, assnItem.Description);

            ObjectHelper.SetObjectsActive(ShowWhenComplete, assnItem.IsComplete);
            ObjectHelper.SetObjectsActive(ShowWhenNotComplete, !assnItem.IsComplete);

            if (assnItem.IsComplete)
            {
                if (OnItemComplete != null)
                {
                    OnItemComplete.Invoke();
                }
            }
            else
            {
                if (OnItemNotComplete != null)
                {
                    OnItemNotComplete.Invoke();
                }
            }
        }
    }

}