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
        private Logger m_logger;
        private Panel m_currQuizPanel;

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
            if (m_currQuizPanel != null)
            {
                m_currQuizPanel.Back();
            }

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

            PanelManager.Instance.Push(panel, driver, onStackClose);
        }

        void PushQuizPanel<D, P>(D quizDriver) where P : Panel
        {
            if (m_currQuizPanel != null)
            {
                m_currQuizPanel.Back();
            }

            m_currQuizPanel = PanelManager.Instance.GetPanel<P>();

            if (m_currQuizPanel == null)
            {
                m_logger.Error(typeof(P).ToString() + " not found.");
                return;
            }

            var curr = m_currQuizPanel;

            PanelManager.Instance.Push(m_currQuizPanel, quizDriver, () =>
            {
                // If the current panel is the one we pushed, clear m_currQuizPanel
                if (m_currQuizPanel == curr &&
                    m_currQuizPanel.Data == curr.Data)
                {
                    m_currQuizPanel = null;
                }
            });

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
                PushQuizPanel<MultipleChoiceQuizMinigameDriver, MultipleChoiceQuizPanel>(quizDriver);
            });
        }
        
        public void OnSelectResponse(QuizPanelMCItem item, MultipleChoiceQuizMinigameDriver driver)
        {
            driver.AttemptsTried++;

            if (item.Response.IsCorrect)
            {
                driver.CorrectAnswer();

                OnResponseCorrect(item, driver);
            }
            else
            {
                driver.IncorrectAnswer();

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
                driver.Fail();

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
                PushQuizPanel<FreeResponseMinigameDriver, FreeResponseQuizPanel>(quizDriver);
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
                driver.CorrectAnswer();

                OnAttemptCorrect(attempt, driver);
            }
            else
            {
                driver.IncorrectAnswer();

                if (driver.Quiz.MaximumAttempts > 0 && driver.AttemptsTried >= driver.Quiz.MaximumAttempts)
                {
                    driver.Fail();

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