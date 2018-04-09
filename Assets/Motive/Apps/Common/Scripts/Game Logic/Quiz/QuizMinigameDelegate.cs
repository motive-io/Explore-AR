// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Globalization;
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using System;
using System.Linq;
using UnityEngine.Events;
using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Handles UI elements for the Quiz minigame. 
    /// 
    /// Can set some preferences on how players will interact with the Quiz (location, etc.)
    /// </summary>
    public class QuizMinigameDelegate : SingletonComponent<QuizMinigameDelegate>, IMultipleChoiceQuizDelegate, IFreeResponseQuizDelegate
    {
        public const string k_quizStack = "quiz_stack";

        private Logger m_logger;
        public UnityEvent OnAction;

        /// <summary>
        /// The quiz panel should register a callback when pushed.
        /// </summary>
        public UnityEvent AttemptsExhausted { get; set; }


        protected override void Awake()
        {
            base.Awake();
            m_logger = new Logger(this);
            AttemptsExhausted = new UnityEvent();
        }

        public void PushSuccessPanel(IPlayerTaskDriver driver)
        {
            var panel = PanelManager.Instance.GetPanel<QuizSuccessPanel>();
            if (panel == null)
            {
                m_logger.Error(typeof(QuizSuccessPanel).ToString() + " not found.");
                return;
            }

            Action onStackClose = () =>
            {
                TaskManager.Instance.Complete(driver, false);
            };

            PanelManager.Instance.Push(k_quizStack, panel, driver, onStackClose);
        }


        //////////////////////////////////////////////////////////////
        //      MULTIPLE CHOICE QUIZ
        //////////////////////////////////////////////////////////////
        #region 
        public void Action(MultipleChoiceQuizMinigameDriver quizDriver)
        {
            if (OnAction != null)
            {
                OnAction.Invoke();
            }

            ThreadHelper.Instance.CallOnMainThread(() =>
            {
                var panel = PanelManager.Instance.GetPanel<MultipleChoiceQuizPanel>();
                if (panel == null)
                {
                    m_logger.Error(typeof(MultipleChoiceQuizPanel).ToString() + " not found.");
                    return;
                }

                PanelManager.Instance.Push(k_quizStack, panel, quizDriver);
            });
        }


        public void OnSelectResponse(QuizPanelMCItem item, MultipleChoiceQuizMinigameDriver driver)
        {
            driver.AttemptsTried++;

            if (item.Response.IsCorrect)
            {
                OnResponseCorrect(item, driver);
            }
            else
            {
                OnResponseWrong(item, driver);
            }

            if (item.Response.Event != null && item.Response.Event.Count() > 0)
            {
                foreach (var e in item.Response.Event)
                {
                    driver.TaskDriver.ActivationContext.FireEvent(e);
                }
            }
        }

        public void OnResponseWrong(QuizPanelMCItem item, MultipleChoiceQuizMinigameDriver driver)
        {
            item.SetCanSelect(false);

            if (driver.Quiz.MaximumAttempts > 0 && driver.AttemptsTried >= driver.Quiz.MaximumAttempts)
            {
                OnAttemptsExhausted(driver);
            }
        }

        public void OnResponseCorrect(QuizPanelMCItem item, MultipleChoiceQuizMinigameDriver driver)
        {
            PushSuccessPanel(driver.TaskDriver);
        }

        public void OnAttemptsExhausted(MultipleChoiceQuizMinigameDriver driver)
        {
            // for multiple choice we don't do anything, the correct answer will reveal itself
        }
        #endregion



        //////////////////////////////////////////////////////////////
        //      FREE RESPONSE QUIZ
        //////////////////////////////////////////////////////////////
        public void Action(FreeResponseMinigameDriver quizDriver)
        {
            if (OnAction != null)
            {
                OnAction.Invoke();
            }

            ThreadHelper.Instance.CallOnMainThread(() =>
            {
                var panel = PanelManager.Instance.GetPanel<FreeResponseQuizPanel>();
                if (panel == null)
                {
                    m_logger.Error(typeof(FreeResponseQuizPanel).ToString() + " not found.");
                    return;
                }

                PanelManager.Instance.Push(k_quizStack, panel, quizDriver);
            });
        }

        public void SubmitAnswer(string _attempt, FreeResponseMinigameDriver driver, Action<string> onIncorrect = null)
        {
            var isCorrect = false;
            var attempt = _attempt;

            driver.AttemptsTried++;

            if (string.IsNullOrEmpty(_attempt))
            {
                attempt = "";
            }
            attempt = attempt.Trim();

            if (driver.Quiz == null || driver.Quiz.Answer.AnswerText == null || driver.Quiz.Answer.AnswerText.Count() <= 0)
            {
                throw new MissingFieldException("Quiz has no answer text attached.");
            }

            if (driver.Quiz.Answer is StrictMatchAnswer)
            {
                var answerObj = driver.Quiz.Answer as StrictMatchAnswer;
                isCorrect = CheckStrictMatch(attempt, answerObj);
            }
            else if (driver.Quiz.Answer is LooseMatchAnswer)
            {
                // not implemented
                //var answerObj = driver.Quiz.Answer as LooseMatchAnswer;
                //isCorrect = CheckLooseMatch(attempt, answerObj);
            }
            else if (driver.Quiz.Answer is TextMustContainAnswer)
            {
                var answerObj = driver.Quiz.Answer as TextMustContainAnswer;
                isCorrect = CheckMustContainMatch(attempt, answerObj);
            }
            else
            {
                throw new Exception("Unable to cast Quiz answer.");
            }

            if (isCorrect)
            {
                OnAttemptCorrect(attempt, driver);
            }
            else
            {

                if (driver.Quiz.MaximumAttempts > 0 && driver.AttemptsTried >= driver.Quiz.MaximumAttempts)
                {
                    OnAttemptsExhausted(driver);
                }
                if (onIncorrect != null)
                {
                    onIncorrect.Invoke(attempt);
                }

            }
        }

        public void OnAttemptsExhausted(FreeResponseMinigameDriver driver)
        {
            if (AttemptsExhausted != null)
            {
                AttemptsExhausted.Invoke();
            }
        }


        /// <summary>
        /// Player answer attempt must match one of the answer texts, with enfore case optionally true.
        /// Cleans strings of spaces on ends.
        /// </summary>
        /// <param name="attempt"></param>
        /// <param name="answerObj"></param>
        /// <returns></returns>
        public bool CheckStrictMatch(string attempt, StrictMatchAnswer answerObj)
        {
            if (string.IsNullOrEmpty(attempt))
            {
                attempt = "";
            }

            var isCorrect = false;
            foreach (var locText in answerObj.AnswerText)
            {
                var answer = LocalizedText.GetText(locText).Trim();

                if (answerObj.EnforceCase)
                {
                    isCorrect = (answer == attempt);
                }
                else
                {
                    var answer_lower = answer.ToLower();
                    var attempt_lower = attempt.ToLower();
                    isCorrect = (answer_lower == attempt_lower);
                }
                if (isCorrect) break;
            }

            return isCorrect;
        }

        public bool CheckLooseMatch(string attempt, LooseMatchAnswer answerObj)
        {
            throw new NotImplementedException();
        }

        public bool CheckMustContainMatch(string attempt, TextMustContainAnswer answerObj)
        {
            if (string.IsNullOrEmpty(attempt))
            {
                attempt = "";
            }

            var isCorrect = false;
            foreach (var locText in answerObj.AnswerText)
            {
                var answer = LocalizedText.GetText(locText).Trim();

                if (answerObj.EnforceCase)
                {
                    isCorrect = attempt.Contains(answer);
                }
                else
                {
                    var answer_lower = answer.ToLower();
                    var attempt_lower = attempt.ToLower();
                    isCorrect = attempt_lower.Contains(answer_lower);
                }
                if (isCorrect) break;
            }

            return isCorrect;
        }

        public void OnAttemptCorrect(string _attempt, FreeResponseMinigameDriver driver)
        {
            PushSuccessPanel(driver.TaskDriver);
        }
    }

}