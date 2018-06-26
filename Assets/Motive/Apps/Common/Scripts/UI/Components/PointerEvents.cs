// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Exposes various pointer events.
    /// </summary>
    public class PointerEvents : MonoBehaviour,
        IPointerDownHandler,
        IPointerExitHandler,
        IPointerClickHandler
    {
        public UnityEvent OnDown;
        public UnityEvent OnUpOrExit;
        public UnityEvent OnDoubleClick;

        public float DoubleClickSpeed = 0.5f;

        //bool m_isDown;
        bool m_wasDown;
        float m_lastClick;

        void OnMouseDown()
        {
            //m_isDown = true;
            m_wasDown = true;

            Call(OnDown);
        }

        void OnMouseUp()
        {
            //m_isDown = false;

            Call(OnUpOrExit);
        }

        void OnMouseExit()
        {
            //m_isDown = false;

            if (m_wasDown)
            {
                m_wasDown = false;

                Call(OnUpOrExit);
            }
        }

        void Call(UnityEvent onEvent)
        {
            if (onEvent != null)
            {
                onEvent.Invoke();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (m_wasDown)
            {
                m_wasDown = false;

                Call(OnUpOrExit);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            //m_isDown = true;
            m_wasDown = true;

            Call(OnDown);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Time.time - m_lastClick < DoubleClickSpeed)
            {
                Call(OnDoubleClick);
            }

            m_lastClick = Time.time;
        }
    }
}