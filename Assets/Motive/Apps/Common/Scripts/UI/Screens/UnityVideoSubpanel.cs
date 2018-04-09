// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive.Unity.UI
{
    /// <summary>
    /// VideoSubpanel that uses Unity's video player.
    /// </summary>
    public class UnityVideoSubpanel : VideoSubpanel
    {
        Logger m_logger;
        VideoPlayer m_videoPlayer;
        bool m_internalUpdate;

        public AudioSource AudioSource;

        bool m_paused;
        bool m_playOnPrepare;
        double m_pauseTime;

        public override bool Loop
        {
            get
            {
                return m_videoPlayer.isLooping;
            }
            set
            {
                if (m_videoPlayer)
                {
                    m_videoPlayer.isLooping = value;
                }
            }
        }

        public override float Volume
        {
            get
            {
                return AudioSource.volume;
            }
            set
            {
                AudioSource.volume = value;
            }
        }

        public override float AspectRatio
        {
            get
            {
                if (m_videoPlayer != null)
                {
                    if (m_videoPlayer.clip != null)
                    {
                        return m_videoPlayer.clip.width / m_videoPlayer.clip.height;
                    }
                    else if (m_videoPlayer.texture != null)
                    {
                        return (float)m_videoPlayer.texture.width / (float)m_videoPlayer.texture.height;
                    }
                }

                return 1f;
            }
        }

        void Awake()
        {
            m_logger = new Logger(this);

            m_videoPlayer = GetComponentInChildren<VideoPlayer>();
            m_videoPlayer.loopPointReached += m_videoPlayer_loopPointReached;
            m_videoPlayer.prepareCompleted += m_videoPlayer_prepareCompleted;
            m_videoPlayer.errorReceived += m_videoPlayer_errorReceived;

            //Set Audio Output to AudioSource
            m_videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

            if (!AudioSource)
            {
                AudioSource = GetComponent<AudioSource>();
            }

            if (PlaybackCompleted == null)
            {
                PlaybackCompleted = new UnityEvent();
            }

            if (ClipLoaded == null)
            {
                ClipLoaded = new UnityEvent();
            }
        }

        void m_videoPlayer_errorReceived(VideoPlayer source, string message)
        {
            m_logger.Error("Caught error: {0}", message);
        }

        void m_videoPlayer_prepareCompleted(VideoPlayer source)
        {
            m_logger.Debug("PrepareCompleted");

            ClipLoaded.Invoke();

            if (m_paused)
            {
                m_videoPlayer.time = m_pauseTime;
            }

            if (m_playOnPrepare)
            {
                m_playOnPrepare = false;
                Play();
            }
        }

        public override void Play()
        {
            m_logger.Debug("Play - isPrepared={0}", m_videoPlayer.isPrepared);

            if (m_videoPlayer.isPrepared)
            {
                m_videoPlayer.Play();

                if (m_paused)
                {
                    m_videoPlayer.time = m_pauseTime;
                    m_paused = false;
                }
            }
            else
            {
                m_playOnPrepare = true;
                m_videoPlayer.Prepare();
            }
        }

        public override void Play(string url)
        {
            m_paused = false;

            m_logger.Debug("Play url={0}", url);

            if (url != null)
            {
                var localUrl = WebServices.Instance.MediaDownloadManager.GetPathForItem(url);

                m_videoPlayer.url = localUrl;

                m_videoPlayer.controlledAudioTrackCount = 1;
                //Assign the Audio from Video to AudioSource to be played
                m_videoPlayer.EnableAudioTrack(0, true);
                m_videoPlayer.SetTargetAudioSource(0, AudioSource);

                m_logger.Debug("Play - isPrepared={0}", m_videoPlayer.isPrepared);

                if (m_videoPlayer.isPrepared)
                {
                    m_videoPlayer.Play();
                }
                else
                {
                    m_playOnPrepare = true;

                    m_videoPlayer.Prepare();
                }
            }
        }

        void m_videoPlayer_loopPointReached(VideoPlayer source)
        {
            OnComplete();
        }

        public override void Pause()
        {
            m_pauseTime = m_videoPlayer.time;
            m_paused = true;

            m_videoPlayer.Pause();
        }

        public override void Stop()
        {
            m_paused = false;

            m_videoPlayer.Stop();
        }

        float GetDurationSeconds(VideoPlayer player)
        {
            if (!player)
            {
                return 0f;
            }

            if (player.frameRate > 0)
            {
                return player.frameCount / player.frameRate;
            }

            return 0f;
        }

        double GetPosPct(VideoPlayer player)
        {
            return (double)player.frame / (double)player.frameCount;
        }

        public override void UpdatePosition(float pos)
        {
            m_videoPlayer.frame = (long)(pos * m_videoPlayer.frameCount);
        }

        public override float Position
        {
            get { return (float)(GetDurationSeconds(m_videoPlayer) * GetPosPct(m_videoPlayer)); }
        }

        public override float Duration
        {
            get { return GetDurationSeconds(m_videoPlayer); }
        }

        public override bool IsPlaying
        {
            get { return m_videoPlayer.isPlaying; }
        }
    }

}