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

        private Action m_onComplete;

        public abstract float AspectRatio { get; }
        public abstract float Position { get; }
        public abstract float Duration { get; }
        public abstract bool IsPlaying { get; }

        public abstract bool Loop { get; set; }
        public abstract float Volume { get; set; }

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