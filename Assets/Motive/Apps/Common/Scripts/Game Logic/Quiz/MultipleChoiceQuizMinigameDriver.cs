// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Minigame driver for multiple choice quiz minigames.
    /// </summary>
    public class MultipleChoiceQuizMinigameDriver : QuizMinigameDriverBase<MultipleChoiceQuiz>
    {
        public MultipleChoiceQuizMinigameDriver(IPlayerTaskDriver taskDriver, MultipleChoiceQuiz quiz) : base(taskDriver, quiz)
        {

        }

        public override void Action()
        {
            QuizMinigameDelegate.Instance.Action(this);
        }
    }
}