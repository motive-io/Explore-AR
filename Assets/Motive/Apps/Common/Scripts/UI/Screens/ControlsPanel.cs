// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Motive.UI.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    ///     Controls that are to be shared across video and audio panels.
    /// </summary>
    public class ControlsPanel : Panel
    {
        private readonly string _pauseCharacter = "";
        private readonly string _playCharacter = "";
        private bool _isPaused;
        public AudioSubpanel AudioSubPanel;

        public Dictionary<Type, bool> EnabledPanels;
        public GameObject FullScreenVideoToggle;
        public bool IsFullscreenDisabled;

        [CanBeNull] public Action OnOpen;

        public Action OnPause;
        public Action OnPlay;
        public Text PlayPauseText;

        public UserInputSlider Slider;
        public Text Time;


        public VideoSubpanel VideoSubPanel;

        public void Disable(object toDisable)
        {
            SetStatus(toDisable, false);
        }

        public void DisableFullScreen()
        {
            IsFullscreenDisabled = true;
            FullScreenVideoToggle.SetActive(false);
        }

        public void Enable(object toEnable)
        {
            SetStatus(toEnable, true);
        }

        public void FullScreen()
        {
            if (!IsFullscreenDisabled)
            {
                VideoSubPanel.SetFullScreen(true);
            }
        }

        public bool IsEnabled(Type toCheck)
        {
            bool panelIsEnabled;

            if (!EnabledPanels.ContainsKey(toCheck))
            {
                return false;
            }

            EnabledPanels.TryGetValue(toCheck, out panelIsEnabled);
            return panelIsEnabled;
        }

        public void OnSeek(UserInputSlider slider)
        {
            if (VideoSubPanel != null && IsEnabled(typeof(VideoSubpanel)))
            {
                VideoSubPanel.UpdatePosition(slider.value);
            }

            if (AudioSubPanel == null || !IsEnabled(typeof(AudioSubpanel)))
            {
                return;
            }

            AudioSubPanel.Slider = slider;
            AudioSubPanel.UpdatePosition(slider);
        }

        public void Pause()
        {
            // Pause the player
            if (AudioSubPanel != null && IsEnabled(typeof(AudioSubpanel)))
            {
                AudioSubPanel.Pause();
            }
            if (VideoSubPanel != null && IsEnabled(typeof(VideoSubpanel)))
            {
                VideoSubPanel.Pause();
            }

            // Pause Hook.
            if (OnPause != null)
            {
                OnPause();
            }


            if (PlayPauseText != null)
            {
                PlayPauseText.text = _playCharacter;
            }

            _isPaused = true;
        }

        public void Play()
        {
            // Play the player
            if (AudioSubPanel != null && IsEnabled(typeof(AudioSubpanel)))
            {
                AudioSubPanel.Play();
            }
            if (VideoSubPanel != null && IsEnabled(typeof(VideoSubpanel)))
            {
                VideoSubPanel.Play();
            }

            // Play Hook.
            if (OnPlay != null)
            {
                OnPlay();
            }

            if (PlayPauseText != null)
            {
                PlayPauseText.text = _pauseCharacter;
            }

            _isPaused = false;
        }

        public void PlayPause()
        {
            if (_isPaused)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

        public override void Populate()
        {
            EnabledPanels = new Dictionary<Type, bool>();

            var audioSub = GetComponentInParent<AudioSubpanel>();
            var videoSub = GetComponentInParent<VideoSubpanel>();


            if (audioSub)
            {
                AudioSubPanel = audioSub;
                AudioSubPanel.Slider = Slider;

                if (Time != null)
                {
                    AudioSubPanel.Time = Time;
                }

                // TODO:: Make this happen dynamically by checking property of parent.
                EnabledPanels.Add(typeof(AudioSubpanel), true);
            }
            if (videoSub)
            {
                VideoSubPanel = videoSub;

                if (Time != null)
                {
                    // Time?
                }

                if (FullScreenVideoToggle != null)
                {
                    // The controls are on a video, set them active.
                    if (VideoSubPanel.FullScreen != null)
                    {
                        VideoSubPanel.FullScreen.GetComponentInChildren<VideoControls>().VideoSubpanel = VideoSubPanel;
                        FullScreenVideoToggle.SetActive(true);
                    }
                }

                VideoSubPanel.time = Time;

                //TODO:: Make this happen dynamically by checking property of parent.
                EnabledPanels.Add(typeof(VideoSubpanel), true);
            }


            // Open Hook.
            if (OnOpen != null)
            {
                OnOpen();
            }

            base.Populate();
        }


        public void SetStatus(object toSet, bool status)
        {
            if (EnabledPanels == null)
            {
                Console.WriteLine("You are calling set status before the prefab is instantiated.");
                return;
            }

            if (!EnabledPanels.ContainsKey(toSet.GetType()))
            {
                EnabledPanels.Add(toSet.GetType(), status);
            }

            EnabledPanels[toSet.GetType()] = status;
        }
    } 
}