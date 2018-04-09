// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Globalization;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    public class FreeResponseQuizPanel : QuizPanel<FreeResponseMinigameDriver>
    {
        public InputField AnswerInput;

        public GameObject ShowAnswerContainer;
        public Text AnswerText;

        public override void DePopulate()
        {
            this.AnswerInput.text = null;
            this.AnswerText.text = null;

            if (ShowAnswerContainer)
            {
                ObjectHelper.SetObjectActive(ShowAnswerContainer, false);
            }
        }

        public override void Populate(FreeResponseMinigameDriver data)
        {
            base.Populate(data);
        }

        /// <summary>
        /// When the "submit answer" button is pressed.
        /// </summary>
        public virtual void Submit()
        {
            if (AnswerInput == null)
            {
                m_logger.Error(string.Format("AnswerInput not found on class {0}.", typeof(FreeResponseQuizPanel).ToString()));
                return;
            }

            var _this = this;
            Action<string> onIncorrect = (attempt) =>
            {
                _this.AnswerInput.text = null;
            };

            QuizMinigameDelegate.Instance.SubmitAnswer(AnswerInput.text, Data, onIncorrect);
        }

        public override void DidPush()
        {
            QuizMinigameDelegate.Instance.AttemptsExhausted.AddListener(AttemptsExhausted_Callback);

            base.DidPush();
        }

        public override void DidPop()
        {
            QuizMinigameDelegate.Instance.AttemptsExhausted.RemoveAllListeners();

            base.DidPop();
        }

        public void AttemptsExhausted_Callback()
        {
            if (Data == null || Data.Quiz == null
                || Data.Quiz.Answer == null || Data.Quiz.Answer.AnswerText == null
                || Data.Quiz.Answer.AnswerText.Count() <= 0)
            {
                throw new Exception("No Answers assigned to this Quiz");
            }

            var answerString = LocalizedText.GetText(
                                Data.Quiz.Answer.AnswerText.FirstOrDefault());
            if (string.IsNullOrEmpty(answerString)) { throw new Exception("Unable to parse answer string."); }

            if (!AnswerText)
            {
                m_logger.Warning(string.Format("No Answer Text object on {0}.", typeof(FreeResponseQuizPanel).ToString()));
            }
            else
            {
                AnswerText.text = answerString;
            }

            if (ShowAnswerContainer)
            {
                ObjectHelper.SetObjectActive(ShowAnswerContainer, true);
            }

        }

        private void Update()
        {
            var speed = 1.0f; //how fast it shakes
            var amount = 1.0f; //how much it shakes

            var pos = this.gameObject.transform.position;
            var delta = Mathf.Sin(Time.time * speed);
            this.gameObject.transform.position.Set(pos.x + delta, pos.y, pos.z);
        }
    }

}