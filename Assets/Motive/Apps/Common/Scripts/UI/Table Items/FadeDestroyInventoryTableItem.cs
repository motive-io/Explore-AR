// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Table item that fades in and then disappears.
    /// </summary>
    class FadeDestroyInventoryTableItem : InventoryTableItem
    {
        public enum FadeShowDestroy
        {
            Hidden,
            FadeIn,
            Show,
            FadeOut,
            Destroy
        }

        private FadeShowDestroy m_state { set; get; }
        public FadeShowDestroy state
        {
            get { return m_state; }
            set
            {
                m_state = value;
                m_secondsSincePrevState = 0;
            }
        }


        public float FadeDuration = 1f;
        public float ShowDuration = 5f;

        //private DateTime m_startTime;
        private float m_secondsSincePrevState;
        private CanvasGroup m_canvGroup;
        private LayoutElement m_layoutElem;
        private float m_startPrefHeight;
        private bool m_doUpdate;
        private Action m_onDestroy;
        private bool m_hasCalledPopulate;

        public void StartAnimation()
        {
            if (!m_hasCalledPopulate) Populate();

            m_doUpdate = true;
            state = FadeShowDestroy.FadeIn;
        }

        public void StartAnimation(Action OnDestroy)
        {
            m_onDestroy = OnDestroy;
            StartAnimation();
        }

        public void Populate()
        {
            m_hasCalledPopulate = true;
            state = FadeShowDestroy.Hidden;
            m_secondsSincePrevState = 0;
            m_doUpdate = false;

            if (m_canvGroup == null) m_canvGroup = GetComponent<CanvasGroup>();
            if (m_canvGroup)
            {
                m_canvGroup.alpha = 0;
            }


            m_layoutElem = GetComponent<LayoutElement>();

            if (m_layoutElem)
            {
                m_startPrefHeight = m_layoutElem.preferredHeight;
            }

            OnSelected.AddListener(() => state = FadeShowDestroy.FadeOut);
        }

        public override void Populate(Collectible collectible)
        {
            base.Populate(collectible);
            Populate();
        }



        void Update()
        {
            if (!m_doUpdate) return;

            //var totalFinalDuration = FadeDuration + ShowDuration + FadeDuration;
            m_secondsSincePrevState += Time.deltaTime;

            switch (m_state)
            {
                case FadeShowDestroy.Hidden:
                    m_canvGroup.alpha = 0;
                    break;
                case FadeShowDestroy.FadeIn:
                    var fadeInT = m_secondsSincePrevState / FadeDuration;
                    FadeInUpdate(fadeInT);
                    break;
                case FadeShowDestroy.Show:
                    var showT = m_secondsSincePrevState / ShowDuration;
                    ShowUpdate(showT);
                    break;
                case FadeShowDestroy.FadeOut:
                    var fadeOutT = m_secondsSincePrevState / FadeDuration;
                    FadeOutUpdate(fadeOutT);
                    PrefHeightUpdate(fadeOutT);
                    break;
                case FadeShowDestroy.Destroy:
                    DestroyUpdate();
                    break;
            }
        }


        // for each of these, 0 <= t <= 1
        private void FadeInUpdate(float t)
        {
            if (t >= 1)
            {
                state = FadeShowDestroy.Show;
                return;
            }
            if (m_canvGroup) m_canvGroup.alpha = Mathf.Lerp(0, 1, t);
        }

        private void ShowUpdate(float t)
        {
            if (t >= 1)
            {
                state = FadeShowDestroy.FadeOut;
            }

        }

        private void FadeOutUpdate(float t)
        {
            if (t >= 1)
            {
                state = FadeShowDestroy.Destroy;
                return;
            }

            if (m_canvGroup) m_canvGroup.alpha = Mathf.Lerp(1, 0, t);
        }

        private void PrefHeightUpdate(float t)
        {
            if (m_layoutElem == null) return;

            m_layoutElem.preferredHeight = m_startPrefHeight - Mathf.Min((m_startPrefHeight * t), m_startPrefHeight);
        }

        private void DestroyUpdate()
        {
            if (m_onDestroy != null)
            {
                m_onDestroy();
            }
            if (gameObject)
            {
                Destroy(gameObject);
            }
        }


        // todo override Select() to fade and destroy
    }

}