// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Motive.Unity.UI.Effects
{
    public class AnimateFill : MonoBehaviour
    {
        public Image Image;

        public float Start = 0;
        public float End = 1;

        public float Duration = 1;

        public UnityEvent OnComplete;

        bool m_animating;
        float m_startTime;

        // Update is called once per frame
        void Update()
        {
            if (m_animating)
            {
                var pct = (Time.time - m_startTime) / Duration;

                if (pct >= 1)
                {
                    m_animating = false;
                    Image.fillAmount = End;

                    if (OnComplete != null)
                    {
                        OnComplete.Invoke();
                    }
                }
                else
                {
                    Image.fillAmount = (End - Start) * pct;
                }
            }
        }

        public void Run()
        {
            m_startTime = Time.time;
            m_animating = true;
        }

        public void Reset()
        {
            m_animating = false;
            Image.fillAmount = Start;
        }
    }

}