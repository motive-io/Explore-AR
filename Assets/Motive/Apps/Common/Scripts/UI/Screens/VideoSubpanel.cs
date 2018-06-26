// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Media;
using Motive.UI.Framework;
using Motive.Unity.Playables;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Abstract base class for video playback.
    /// </summary>
    public abstract class VideoSubpanel : MonoBehaviour
    {
        public UnityEvent PlaybackCompleted;
        public UnityEvent ClipLoaded;

        public GameObject Embedded;
        public GameObject FullScreen;
        public Text time;
        public bool SwitchModeOnRotate;
        public bool AutoRotateInFullScreen;

        private Action m_onComplete;

        public abstract float AspectRatio { get; }
        public abstract float Position { get; }
        public abstract float Duration { get; }
        public abstract bool IsPlaying { get; }

        public abstract bool Loop { get; set; }
        public abstract float Volume { get; set; }

        bool m_checkedOrientation;
        bool m_isLandscape;

        public virtual void Play(Action onComplete)
        {
            SetOnCompleteHandler(onComplete);

            Play();
        }

        public abstract void Play();

        public abstract void Play(string url);

        public virtual void Play(string url, Action onComplete)
        {
            SetOnCompleteHandler(onComplete);

            Play(url);
        }

        public abstract void Stop();

        public abstract void Pause();

        protected virtual void SetOnCompleteHandler(Action onComplete)
        {
            m_onComplete = onComplete;
        }

        protected virtual void OnComplete()
        {
            if (m_onComplete != null)
            {
                m_onComplete();
            }

            if (PlaybackCompleted != null)
            {
                PlaybackCompleted.Invoke();
            }
        }

        public virtual void Play(MediaItem media, Action onComplete = null)
        {
            if (media != null)
            {
                if (AudioContentPlayer.Instance)
                {
                    AudioContentPlayer.Instance.Pause();
                }

                Play(media.Url, onComplete);
            }
        }

        public void SetFullScreen(bool fullScreen)
        {
            if (FullScreen && Embedded)
            {
                FullScreen.SetActive(fullScreen);
                Embedded.SetActive(!fullScreen);
            }

            if (AutoRotateInFullScreen)
            {
                if (fullScreen)
                {
                    PanelManager.Instance.SetOrientation(ScreenOrientation.AutoRotation);
                }
                else
                {
                    PanelManager.Instance.SetOrientation(PanelManager.Instance.DefaultOrientation);
                }
            }
        }

        public void ToggleFullScreen()
        {
            if (FullScreen && Embedded)
            {
                bool fullScreen = FullScreen.activeSelf;

                SetFullScreen(!fullScreen);
            }
        }

        public void PlayPause()
        {
            if (IsPlaying)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        protected virtual void Update()
        {
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
                    SetFullScreen(true);
                }

                m_isLandscape = isLandscape;
                m_checkedOrientation = true;
            }

            if (time)
            {
                var durationTime = string.Format("{0}:{1:00}", (int)Duration / 60, (int)Duration % 60);
                var prettyTime = string.Format("{0}:{1:00} / {2}", (int)Position / 60, (int)Position % 60, durationTime);
                time.text = prettyTime;

            }
        }

        public abstract void UpdatePosition(float pos);

        internal virtual void Close()
        {
            Stop();

            if (AudioContentPlayer.Instance)
            {
                AudioContentPlayer.Instance.Resume();
            }
        }
    }

}