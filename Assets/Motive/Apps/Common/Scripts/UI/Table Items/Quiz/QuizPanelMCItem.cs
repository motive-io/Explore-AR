// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using Motive.Unity.Utilities;
using System.Linq;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Multiple choice item for quiz.
    /// </summary>
    public class QuizPanelMCItem : TextImageItem
    {
        public MultipleChoiceQuizResponse Response;
        public GameObject[] ActiveWhenDisabled;

        public virtual void Populate(MultipleChoiceQuizResponse response)
        {
            this.Response = response;

            if (!string.IsNullOrEmpty(response.ResponseText.Text)) SetText(response.ResponseText.Text);
            if (!string.IsNullOrEmpty(response.ImageUrl)) SetImage(response.ImageUrl);
        }

        public void SetCanSelect(bool canSelect = true)
        {
            if (ActiveWhenDisabled != null && ActiveWhenDisabled.Count() > 0)
            {
                ObjectHelper.SetObjectsActive(ActiveWhenDisabled, !canSelect);
            }

            this.Selectable = canSelect;
        }
    }

}