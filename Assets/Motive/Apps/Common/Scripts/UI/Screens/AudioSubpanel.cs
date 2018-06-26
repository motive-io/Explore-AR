// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Motive.Core.Media;
using Motive.Unity.Media;
using System;
using UnityEngine.Events;
using Motive.Unity.Utilities;
using Motive;
using Motive.Unity.Playables;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Simple component for handling audio playback for a MediaItem.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class AudioSubpanel : MonoBehaviour
    {
        public UnityEvent PlaybackCompleted;

        public Text Time;
        public UserInputSlider Slider;
        public Button PlayPauseButton;
        public bool Autoplay = true;

        public SoundViewWidget SoundView;

        public string PauseCharacter = "";
        public string PlayCharacter = "";

        IAudioPlayer m_player;
        Action m_onComplete;

        public void Play(string url, Action onComplete = null)
        {
            m_onComplete = onComplete;

            if (url != null)
            {
                m_player = Platform.Instance.CreateAudioPlayer(url);

                if (Autoplay)
                {
                    m_player.Play((success) =>
                    {
                        FinishPlaying();
                    });
                }

                if (SoundView)
                {
                    SoundView.Play(m_player);
                }

                if (AudioContentPlayer.Instance)
                {
                    AudioContentPlayer.Instance.Pause();
                }
            }
        }

        public void Play(MediaItem mediaItem, Action onComplete = null)
        {
            if (mediaItem != null)
            {
                Play(mediaItem.Url, onComplete);
            }
        }

        private void FinishPlaying()
        {
            ThreadHelper.Instance.CallOnMainThread(() =>
                {
                    if (PlaybackCompleted != null)
                    {
                        PlaybackCompleted.Invoke();
                    }

                    if (m_onComplete != null)
                    {
                        var toCall = m_onComplete;
                        m_onComplete = null;
                        toCall();
                    }
                });
        }

        public void Play(Action onComplete)
        {
            m_onComplete = onComplete;

            Play();
        }

        public void Play()
        {
            if (AudioContentPlayer.Instance)
            {
                AudioContentPlayer.Instance.Pause();
            }

            m_player.Play();
        }

        public void Pause()
        {
            m_player.Pause();
        }

        public void PlayPause()
        {
            if (m_player.IsPlaying)
            {
                m_player.Pause();
            }
            else
            {
                m_player.Play();
            }
        }

        public void UpdatePosition()
        {
            if (Slider.IsUserUpdate)
            {
                var pos = Slider.value * m_player.Duration.TotalSeconds;

                m_player.Position = TimeSpan.FromSeconds(pos);
            }
        }

        /// <summary>
        /// Updates audio position based on the new audio controls.
        /// </summary>
        /// <param name="slider"></param>
        public void UpdatePosition(UserInputSlider slider)
        {
            if (!slider.IsUserUpdate) return;

            var pos = slider.value * m_player.Duration.TotalSeconds;

            m_player.Position = TimeSpan.FromSeconds(pos);
        }

        // Update is called once per frame
        void Update()
        {
            if (m_player != null && m_player.Duration.TotalSeconds > 0)
            {
                var pct = m_player.Position.TotalSeconds / m_player.Duration.TotalSeconds;

                if (Slider && !Slider.IsUserUpdate)
                {
                    Slider.value = (float)pct;
                }

                if (Time)
                {
                    Time.text = string.Format("{0}  /  {1}",
                        StringFormatter.FormatTimespan(m_player.Position),
                        StringFormatter.FormatTimespan(m_player.Duration));
                }

                if (PlayPauseButton)
                {
                    PlayPauseButton.GetComponentInChildren<Text>().text = m_player.IsPlaying ? PauseCharacter : PlayCharacter;
                }
            }
        }

        public void Stop()
        {
            if (m_player != null)
            {
                m_player.Stop();
                m_player.Dispose();

                m_player = null;
            }

            if (SoundView)
            {
                SoundView.Stop();
            }
        }

        public void Close()
        {
            Stop();

            if (AudioContentPlayer.Instance)
            {
                AudioContentPlayer.Instance.Resume();
            }
        }
    }

}