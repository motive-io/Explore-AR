// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Media;
using System;

using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive.Unity.Media
{
    public class ManagedAudioPlayer : IAudioPlayer 
    {
        IAudioPlayer m_player;
        bool m_isPlaying;
        Logger m_logger;

        public bool AllowBackground { get; private set; }

        public ManagedAudioPlayer(IAudioPlayer player, bool allowBackground)
        {
            m_logger = new Logger(this);

            AllowBackground = allowBackground;

            m_player = player;
            m_player.AudioEnded += M_player_AudioEnded;

            Platform.Instance.OnPauseAudio.AddListener(PauseAudio);
            Platform.Instance.OnResumeAudio.AddListener(ResumeAudio);
        }

        void PauseAudio()
        {
            m_logger.Debug("PauseAudio player.isPlaying={0} isPlaying={1}",
                m_player.IsPlaying, m_isPlaying);
            
            if (m_player.IsPlaying)
            {
                m_player.Pause();
            }
        }

        void ResumeAudio()
        {
            m_logger.Debug("ResumeAudio player.isPlaying={0} isPlaying={1}",
                m_player.IsPlaying, m_isPlaying);
            
            if (m_isPlaying)
            {
                m_player.Play();
            }
        }

        void M_player_AudioEnded (object sender, EventArgs e)
        {
            m_logger.Debug("AudioEnded player.isPlaying={0} isPlaying={1}",
                m_player.IsPlaying, m_isPlaying);
            
            m_isPlaying = false;
        }

        //
        // Properties
        //
        public TimeSpan Duration 
        {
            get { return m_player.Duration; }
        }

        public bool IsPlaying
        {
            get { return m_player.IsPlaying; }
        }

        public bool Loop 
        {
            get { return m_player.Loop; }
            set { m_player.Loop = value; }
        }

        public float Pitch
        {
            get { return m_player.Pitch; }
            set { m_player.Pitch = value; }
        }

        public TimeSpan Position
        {
            get { return m_player.Position; }
            set { m_player.Position = value; }
        }

        public Uri Source 
        {
            get { return m_player.Source; }
        }

        public float Volume 
        {
            get { return m_player.Volume; }
            set { m_player.Volume = value; }
        }

        //
        // Methods
        //
        public void GetOutputSamples (int channel, double[] samples)
        {
            m_player.GetOutputSamples(channel, samples);
        }

        public void Pause ()
        {
            m_logger.Debug("Pause player.isPlaying={0} isPlaying={1}",
                m_player.IsPlaying, m_isPlaying);
            
            m_isPlaying = false;
            m_player.Pause();
        }

        public void Play (Action<bool> onComplete)
        {
            m_logger.Debug("Play(onComplete) player.isPlaying={0} isPlaying={1}",
                m_player.IsPlaying, m_isPlaying);
            
            m_isPlaying = true;
            m_player.Play(onComplete);
        }

        public void Play ()
        {
            m_logger.Debug("Play player.isPlaying={0} isPlaying={1}",
                m_player.IsPlaying, m_isPlaying);
            
            m_isPlaying = true;
            m_player.Play();
        }

        public void Stop ()
        {
            m_logger.Debug("Stop player.isPlaying={0} isPlaying={1}",
                m_player.IsPlaying, m_isPlaying);
            
            m_isPlaying = false;
            m_player.Stop();
        }

        //
        // Events
        //
        public event EventHandler AudioFailed
        {
            add 
            { 
                lock (m_player)
                {
                    m_player.AudioFailed += value;
                }
            }
            remove
            {
                lock (m_player)
                {
                    m_player.AudioFailed -= value;
                }
            }
        }

        public event EventHandler AudioEnded
        {
            add 
            { 
                lock (m_player)
                {
                    m_player.AudioEnded += value;
                }
            }
            remove
            {
                lock (m_player)
                {
                    m_player.AudioEnded -= value;
                }
            }
        }

        public void Dispose()
        {
            Platform.Instance.OnPauseAudio.RemoveListener(PauseAudio);
            Platform.Instance.OnResumeAudio.RemoveListener(ResumeAudio);

            m_player.Dispose();
        }
    }
}
