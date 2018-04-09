// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections;
using System.Collections.Generic;
using Motive.UI.Framework;
using UnityEngine;

namespace Motive.Unity.UI
{
    public class ControlsSubPanel : Panel
    {
        private bool m_isPaused;
        public AudioSubpanel m_audioSubpanel;
        public VideoSubpanel m_videoSubpanel;
        private Action m_onClose;
        private Action m_onOpen;

        private void Pause()
        {
            // Pause the player
            if (m_audioSubpanel != null)
            {
                m_audioSubpanel.Pause();
            }
            if (m_videoSubpanel != null)
            {
                m_videoSubpanel.Pause();
            }

            m_isPaused = true;
        }

        public void PlayPause()
        {
            if (m_isPaused)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

        private void Play()
        {
            // Play the player
            if (m_audioSubpanel != null)
            {
                m_audioSubpanel.Play();
            }
            if (m_videoSubpanel != null)
            {
                m_videoSubpanel.Play();
            }

            m_isPaused = false;
        }
    }
}