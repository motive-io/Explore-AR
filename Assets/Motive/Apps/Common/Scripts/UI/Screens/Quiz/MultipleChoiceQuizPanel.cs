// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Gaming;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Panel for multiple choice quizzes.
    /// </summary>
    public class MultipleChoiceQuizPanel : QuizPanel<MultipleChoiceQuizMinigameDriver>
    {
        public Table TablePanel;
        public QuizPanelMCItem ResponseItem;

        protected override void Awake()
        {
            TablePanel = GetComponentInChildren<Table>();

            base.Awake();
        }

        public override void Populate(MultipleChoiceQuizMinigameDriver data)
        {
            base.Populate(data); // do this first

            foreach (var response in data.Quiz.Responses)
            {
                var item = TablePanel.AddItem(ResponseItem);

                item.Populate(response);

                item.OnSelected.AddListener(() => { OnSelect(item); });
            }

        }

        protected virtual void OnSelect(QuizPanelMCItem item)
        {
            QuizMinigameDelegate.Instance.OnSelectResponse(item, this.Data);
        }

        public override void DePopulate()
        {
            TablePanel.Clear();
        }
    }

}