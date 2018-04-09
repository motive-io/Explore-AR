// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Base panel component for quiz minigames.
    /// </summary>
    /// <typeparam name="Driver"></typeparam>
    /// <typeparam name="Quiz"></typeparam>
    public class QuizComponentBase<Driver, Quiz> : PanelComponent<Driver>
        where Driver : QuizMinigameDriverBase<Quiz>
        where Quiz : QuizBase
    {
        public Text Title;
        public GameObject TitleLayoutObject;

        public Text Question;
        public GameObject QuestionLayoutObject;

        public RawImage QuestionImage;
        public GameObject QuestionImageLayoutObject;

        string m_defaultButtonText;
        string m_defaultTitleText;

        protected override void Awake()
        {
            base.Awake();
            if (Title)
            {
                m_defaultTitleText = Title.text;
            }
        }

        public override void Populate(Driver quizDriver)
        {
            DePopulate();

            base.Populate(quizDriver);
            // todo: all this 

            if (QuestionLayoutObject && QuestionImage && !string.IsNullOrEmpty(quizDriver.ImageUrl))
            {
                SetImage(QuestionImageLayoutObject, QuestionImage, quizDriver.ImageUrl);
            }
            else
            {
                ObjectHelper.SetObjectActive(QuestionImageLayoutObject, false);
            }


            if (Question)
            {
                Question.text = quizDriver.Quiz.Question.Text;

                if (QuestionLayoutObject)
                {
                    QuestionLayoutObject.SetActive(!string.IsNullOrEmpty(quizDriver.Quiz.Question.Text));
                }
            }
            else
            {
                ObjectHelper.SetObjectActive(QuestionLayoutObject, false);
            }


            if (Title)
            {
                Title.text = quizDriver.TaskDriver.Task.Title;
            }
            else
            {
                Title.text = m_defaultTitleText;
            }

            if (TitleLayoutObject)
            {
                TitleLayoutObject.SetActive(!string.IsNullOrEmpty(quizDriver.TaskDriver.Task.Title));
            }

        }



        public override void DidHide()
        {
            DePopulate();

            base.DidHide();
        }

        public void DePopulate()
        {
            if (Question)
            {
                Question.text = null;
            }

            if (QuestionImage)
            {
                QuestionImage.texture = null;
            }

            if (QuestionLayoutObject)
            {
                ObjectHelper.SetObjectActive(QuestionImageLayoutObject, false);
            }
        }
    }

}