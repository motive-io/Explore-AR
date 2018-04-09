// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Base class driver for free response minigames.
    /// </summary>
    public class FreeResponseMinigameDriver : QuizMinigameDriverBase<FreeResponseQuiz>
    {
        public FreeResponseMinigameDriver(IPlayerTaskDriver taskDriver, FreeResponseQuiz quiz) : base(taskDriver, quiz)
        {

        }

        public override void Action()
        {
            QuizMinigameDelegate.Instance.Action(this);
        }
    }
}