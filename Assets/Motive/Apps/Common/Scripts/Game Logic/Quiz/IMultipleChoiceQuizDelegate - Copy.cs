// Copyright (c) 2018 RocketChicken Interactive Inc.

using Motive.Unity.UI;

namespace Motive.Unity.Gaming
{
    public interface IMultipleChoiceQuizDelegate
    {
        void Action(MultipleChoiceQuizMinigameDriver quizDriver);
        void OnSelectResponse(QuizPanelMCItem item, MultipleChoiceQuizMinigameDriver driver);
        void OnResponseWrong(QuizPanelMCItem item, MultipleChoiceQuizMinigameDriver driver);
        void OnResponseCorrect(QuizPanelMCItem item, MultipleChoiceQuizMinigameDriver driver);
        void OnAttemptsExhausted(MultipleChoiceQuizMinigameDriver driver);
    }

}