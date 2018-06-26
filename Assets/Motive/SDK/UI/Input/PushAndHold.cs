// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Motive.UI.Framework
{
    /// <summary>
    /// Fires an event if a UI element has been held for a specified period of time.
    /// </summary>
    public class PushAndHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {

        public UnityEvent OnSelect;

        public float HoldTime = 5f;

        float m_downTime;
        bool m_isHolding;

        // Update is called once per frame
        void Update()
        {
            if (m_isHolding)
            {
                if (Time.time > m_downTime + HoldTime)
                {
                    m_isHolding = false;

                    if (OnSelect != null)
                    {
                        OnSelect.Invoke();
                    }
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!m_isHolding)
            {
                m_isHolding = true;
                m_downTime = Time.time;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            m_isHolding = false;
        }
    }

}