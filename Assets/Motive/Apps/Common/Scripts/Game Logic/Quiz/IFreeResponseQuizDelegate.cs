// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using System;

namespace Motive.Unity.Gaming
{
    public interface IFreeResponseQuizDelegate
    {
        void Action(FreeResponseMinigameDriver quizDriver);
        void OnAttemptsExhausted(FreeResponseMinigameDriver driver);
        void SubmitAnswer(string _attempt, FreeResponseMinigameDriver driver, Action<string> onIncorrect = null);
        void OnAttemptCorrect(string _attempt, FreeResponseMinigameDriver driver);
        
        bool CheckStrictMatch(string attempt, StrictMatchAnswer answerObj);
        bool CheckLooseMatch(string attempt, LooseMatchAnswer answerObj);
        bool CheckMustContainMatch(string attempt, TextMustContainAnswer answerObj);
    } 
}