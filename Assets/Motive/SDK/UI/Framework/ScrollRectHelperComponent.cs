// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.UI.Framework
{
    /// <summary>
    /// Helper component for managing a ScrollRect. 
    /// </summary>
    public class ScrollRectHelperComponent : PanelComponent
    {
        public ScrollRect ScrollRect;

        public bool ResetOnShow;

        public GameObject[] ShowWhenOverflow;
        public GameObject[] ShowWhenOverflowBottom;
        public GameObject[] ShowWhenOverflowTop;

        RectTransform m_content;
        RectTransform m_viewport;

        void Start()
        {
            if (ScrollRect)
            {
                m_content = ScrollRect.content;
                m_viewport = ScrollRect.viewport ? ScrollRect.viewport : (RectTransform)ScrollRect.transform;
            }
        }

        public override void DidShow()
        {
            base.DidShow();

            if (ScrollRect)
            {
                if (ResetOnShow)
                {
                    ScrollRect.verticalNormalizedPosition = 1;
                }
            }
        }

        void Update()
        {
            if (m_content && m_viewport)
            {
                var overflow = (m_content.sizeDelta.y > m_viewport.sizeDelta.y);
                var overflowTop= (m_content.offsetMax.y > 0.0005);
                var overflowBottom = (m_content.offsetMin.y < 0f);

                ObjectHelper.SetObjectsActive(ShowWhenOverflow, overflow);
                ObjectHelper.SetObjectsActive(ShowWhenOverflowBottom, overflowBottom);
                ObjectHelper.SetObjectsActive(ShowWhenOverflowTop, overflowTop);
            }
        }
    }
}
