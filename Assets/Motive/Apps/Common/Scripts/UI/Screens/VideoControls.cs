// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Controls for video playback.
    /// </summary>
    public class VideoControls : PanelComponent<VideoSubpanel>
    {
        public PanelLink FullScreenPanelLink;
        public AspectRatioFitter AspectRatioFitter;
        public string PauseCharacter = "";
        public string PlayCharacter = "";
        public Button PlayPauseButton;
        public Slider Slider;
        public Text Time;
        public Image SliderFill;

        public bool SwitchModeOnRotate;
        public bool AutoRotateInFullScreen;

        [SerializeField]
        private VideoSubpanel m_subpanel;

        bool m_checkedOrientation;
        bool m_isLandscape;

        public override void Populate(VideoSubpanel obj)
        {
            m_subpanel = obj;

            base.Populate(obj);
        }
        
        public void ToggleFullScreen()
        {
            if (FullScreenPanelLink)
            {
                bool fullScreen = FullScreenPanelLink.IsShowing;

                if (fullScreen)
                {
                    ExitFullScreen();
                }
                else
                {
                    EnterFullScreen();
                }
            }
        }

        public void ExitFullScreen()
        {
            if (FullScreenPanelLink)
            {
                FullScreenPanelLink.Back();
            }
        }

        public void EnterFullScreen()
        {
            if (FullScreenPanelLink)
            {
                FullScreenPanelLink.Push(Data);
            }
        }

        public void PlayPause()
        {
            m_subpanel.PlayPause();
        }
        
        private void Update()
        {
            if (Slider)
            {
                Slider.maxValue = m_subpanel.Duration;
            }

            var duration = m_subpanel.Duration;
            var posPct = m_subpanel.Position / m_subpanel.Duration;

            if (SliderFill)
            {
                if (SliderFill.fillAmount < 1f)
                {
                    SliderFill.fillAmount = posPct;
                }
            }

            if (AspectRatioFitter)
            {
                AspectRatioFitter.aspectRatio = m_subpanel.AspectRatio;
            }

            if (m_subpanel == null || !(duration > 0))
            {
                return;
            }
            
            if (Time)
            {
                var durationTime = string.Format("{0}:{1:00}", (int)m_subpanel.Duration / 60, (int)m_subpanel.Duration % 60);
                var prettyTime = string.Format("{0}:{1:00} / {2}", (int)m_subpanel.Position / 60, (int)m_subpanel.Position % 60, durationTime);

                Time.text = prettyTime;
            }

            if (PlayPauseButton)
            {
                PlayPauseButton.GetComponentInChildren<Text>().text =
                    m_subpanel.IsPlaying ? PauseCharacter : PlayCharacter;
            }

            if (SwitchModeOnRotate)
            {
                if (!m_checkedOrientation)
                {
                    m_isLandscape = (Input.deviceOrientation == DeviceOrientation.LandscapeLeft ||
                                     Input.deviceOrientation == DeviceOrientation.LandscapeRight);
                }

                bool isLandscape = (Input.deviceOrientation == DeviceOrientation.LandscapeLeft ||
                                    Input.deviceOrientation == DeviceOrientation.LandscapeRight);

                if (isLandscape && (!m_checkedOrientation || isLandscape != m_isLandscape))
                {
                    EnterFullScreen();
                }

                m_isLandscape = isLandscape;
                m_checkedOrientation = true;
            }
        }

        public void UpdatePosition()
        {
            if (m_subpanel && Slider)
            {
                m_subpanel.UpdatePosition(Slider.value);
            }
        }
    }
}