// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Motive.Core.Utilities;

namespace Motive.UI.Framework
{
    /// <summary>
    /// Used to create horizontal swipeable menus.
    /// </summary>
    public class SnapToItem : ScrollRect
    {
        public HorizontalLayoutGroup LayoutGroup;
        float m_scrollInertia = 0.3f;
        float m_snapInertia = 0.04f;

        //LayoutElement[] m_layoutElements;
        //ScrollRect m_scrollRect;

        Vector2 m_velocity;
        bool m_dragging;

        bool m_settling;
        Vector2 m_targetPosition;

        void ChooseWinner(PointerEventData eventData)
        {
            Debug.LogFormat("v={0} p={1} w={2}", m_velocity, transform.position, Screen.width);

            var minDist = float.MaxValue;
            var dv = (Vector3)(m_velocity * m_scrollInertia);

            // This puts a cap on the velocity so we don't skip multiple screens.
            // Magic numbers here: can expose as tunables
            var maxMag = viewport.rect.width * 1.3f;

            if (dv.magnitude > maxMag)
            {
                dv = dv.normalized * maxMag;
            }

            var globalPos = transform.position - dv;
            float offset = 0f;
            float winnerOffset = 0f;
            var layoutElements = gameObject.GetComponentsInChildren<LayoutElement>();


            foreach (var element in layoutElements)
            {
                var d = Vector3.Distance(globalPos, element.transform.position);

                if (d < minDist)
                {
                    minDist = d;
                    winnerOffset = offset;
                }

                offset += LayoutGroup.spacing + element.preferredWidth;
            }

            if (content.rect.width < winnerOffset + viewport.rect.width)
            {
                winnerOffset = content.rect.width - viewport.rect.width;
            }

            var pos = content.anchoredPosition;
            pos.x = -winnerOffset;

            m_settling = true;
            m_targetPosition = pos;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            m_lastPos = content.position;
            m_dragging = true;
            m_settling = false;

            base.OnDrag(eventData);
        }

        protected override void Start()
        {
            base.Start();

            LayoutGroup = gameObject.GetComponentInChildren<HorizontalLayoutGroup>();
            //m_scrollRect = gameObject.GetComponentInChildren<ScrollRect>();

            SetContentAnchoredPosition(Vector2.zero);

        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            m_dragging = false;

            base.OnEndDrag(eventData);

            ChooseWinner(eventData);
        }

        Vector3 m_lastPos;

        void Update()
        {
            if (m_dragging)
            {
                var pos = content.position;
                m_velocity = (pos - m_lastPos) / Time.deltaTime;
                m_lastPos = pos;
            }
            else if (m_settling)
            {
                //            Debug.LogFormat("before: t={0} a={1}", m_targetPosition.x, content.anchoredPosition.x);

                var x = MathHelper.Approach(content.anchoredPosition.x, m_targetPosition.x, m_snapInertia, Time.deltaTime);
                var y = MathHelper.Approach(content.anchoredPosition.y, m_targetPosition.y, m_snapInertia, Time.deltaTime);

                SetContentAnchoredPosition(new Vector2(x, y));

                //            Debug.LogFormat("after: x={0} t={1} a={2}", x, m_targetPosition.x, content.anchoredPosition.x);
            }
        }
    }

}