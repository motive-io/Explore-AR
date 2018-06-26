// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Timing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Pops a panel after a set period of time.
    /// </summary>
    public class PopTimerComponent : PanelComponent
    {
        public float Time = 3f;

        UnityTimer m_timer;

        private void Awake()
        {
            if (!Panel)
            {
                Panel = GetComponent<Panel>();
            }
        }

        public override void DidShow()
        {
            if (m_timer != null)
            {
                m_timer.Cancel();
            }

            m_timer = UnityTimer.Call(Time, () =>
            {
                if (Panel)
                {
                    Panel.Back();
                }
            });

            base.DidShow();
        }
    }

}