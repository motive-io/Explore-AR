// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Diagnostics;
using Motive.Core.Media;
using Motive.Core.Models;
using Motive.Core.Timing;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Motive.Unity.Playables
{
    public enum AudioContentRoute
    {
        /// <summary>
        /// Plays an ambient sound. No queuing (multiple sounds
        /// can be played at once).
        /// </summary>
        Ambient,
        /// <summary>
        /// Plays audio in sequence.
        /// </summary>
        Narrator,
        /// <summary>
        /// Plays one audio track at a time, cross-fading between
        /// tracks as they're activated/deactivated.
        /// </summary>
        Soundtrack
    }

    /// <summary>
    /// Handles audio content playback for ambient, soundtrack, and narrator audio.
    /// </summary>
    public class AudioContentPlayer : SingletonComponent<AudioContentPlayer>
    {
        public Object SoundtrackLockObject;

        class PlayerContext
        {
            public LocalizedAudioContent AudioContent { get; private set; }

            public IAudioPlayer Player { get; private set; }

            public string InstanceId { get; private set; }

            public void Play()
            {
                Player.Play();
            }

            public void Pause()
            {
                Player.Pause();
            }

            public void Stop()
            {
                Player.Stop();
            }

            public PlayerContext(string instanceId, IAudioPlayer player, LocalizedAudioContent audioContent)
            {
                this.InstanceId = instanceId;
                this.Player = player;
                this.AudioContent = audioContent;
            }
        }

        public float FadeDuration = 5f;
        public float DuckResumeDelay = 2f;

        private Logger m_logger;

        IAudioPlayerChannel m_channel;

        Dictionary<string, PlayerContext> m_playablePlayers;

        List<PlayerContext> m_soundtrackPlayers;
        DateTime? m_currentSoundtrackPlayTime;
        Dictionary<string, Action> m_onUnpause;
        Timer m_duckResumeTimer;
        Fader m_currentSoundtrackFader;

        bool m_isPlayingSoundtrack;

        public bool IsPaused { get; private set; }

        PlayerContext CurrentSoundtrackPlayer
        {
            get
            {
                if (m_soundtrackPlayers.Count > 0)
                {
                    return m_soundtrackPlayers.Last();
                }

                return null;
            }
        }

        public IAudioPlayer CurrentNarratorPlayer
        {
            get; private set;
        }

        protected override void Awake()
        {
            base.Awake();

            SoundtrackLockObject = new object();

            m_onUnpause = new Dictionary<string, Action>();
            m_logger = new Logger(this);
            m_soundtrackPlayers = new List<PlayerContext>();
            m_playablePlayers = new Dictionary<string, PlayerContext>();
        }

        protected override void Start()
        {
            m_channel = Platform.Instance.CreateAudioPlayerChannel();
        }

        public void Pause(bool immediate = false, Action onPaused = null)
        {
            m_logger.Debug("Pause: isPaused={0}", IsPaused);

            if (!IsPaused)
            {
                IsPaused = true;

                bool hasSoundtrack = false;

                foreach (var p in m_playablePlayers.Values)
                {
                    if (p != CurrentSoundtrackPlayer || immediate)
                    {
                        p.Player.Pause();
                    }
                    else
                    {
                        lock (SoundtrackLockObject)
                        {
                            hasSoundtrack = true;

                            PauseSoundtrack(onPaused);
                        }
                    }
                }

                // If there's a soundtrack player, the "onPaused" call
                // will be handled after fadeout. Otherwise call it here.
                if (!hasSoundtrack)
                {
                    if (onPaused != null)
                    {
                        onPaused();
                    }
                }
            }
        }

        public void Resume(bool immediate = false)
        {
            m_logger.Debug("Resume: isPaused={0}", IsPaused);

            if (IsPaused)
            {
                IsPaused = false;

                foreach (var p in m_playablePlayers.Values)
                {
                    if (p == CurrentSoundtrackPlayer && !immediate)
                    {
                        // FIX:: SOUNDTRACK LOCK OBJECT.
                        lock (SoundtrackLockObject)
                        {
                            ResumeSoundtrack();
                        }
                    }
                    else
                    {
                        p.Play();
                    }
                }

                IEnumerable<Action> toCall = null;

                lock (m_onUnpause)
                {
                    toCall = m_onUnpause.Values.ToArray();
                    m_onUnpause.Clear();
                }

                foreach (var call in toCall)
                {
                    call();
                }
            }
        }

        void DuckExcept(params PlayerContext[] ctxts)
        {
            lock (m_playablePlayers)
            {
                if (m_duckResumeTimer != null)
                {
                    m_duckResumeTimer.Cancel();
                    m_duckResumeTimer = null;
                }

                foreach (var ctxt in m_playablePlayers.Values)
                {
                    if (!ctxts.Contains(ctxt))
                    {
                        Fader.Fade(ctxt.Player, ctxt.Player.Volume, ctxt.AudioContent.Volume * 0.2f, TimeSpan.FromSeconds(0.3), null);
                    }
                }
            }

            Platform.Instance.DuckSystemSounds();
        }

        void DoUnduck()
        {
            lock (m_playablePlayers)
            {
                foreach (var ctxt in m_playablePlayers.Values)
                {
                    Fader.Fade(ctxt.Player, ctxt.Player.Volume, ctxt.AudioContent.Volume, TimeSpan.FromSeconds(0.3), null);
                }
            }
        }

        void Unduck()
        {
            lock (m_playablePlayers)
            {
                if (m_duckResumeTimer != null)
                {
                    m_duckResumeTimer.Cancel();
                    m_duckResumeTimer = null;
                }

                m_duckResumeTimer = Timer.Call(DuckResumeDelay, DoUnduck);
            }

            Platform.Instance.UnduckSystemSounds();
        }

        void FadeInSoundtrack(PlayerContext ctxt, float? duration = null, Action onComplete = null)
        {
            if (m_currentSoundtrackFader != null)
            {
                m_currentSoundtrackFader.Stop();
                m_currentSoundtrackFader = null;
            }

            var fadeDuration = duration ?? FadeDuration;

            var _fader = m_currentSoundtrackFader;

            m_currentSoundtrackFader = Fader.FadeIn(ctxt.Player, TimeSpan.FromSeconds(fadeDuration), ctxt.AudioContent.Volume,
                onFadeIn: () =>
                 {
                    lock (SoundtrackLockObject)
                    {
                        if (_fader == m_currentSoundtrackFader)
                        {
                            m_currentSoundtrackFader = null;
                        }
                    }
                },
                onPlaybackComplete:
                (success) =>
                {
                    StopPlaying(ctxt.InstanceId);

                    if (onComplete != null)
                    {
                        onComplete();
                    }
                });

        }

        void FadeOutSoundtrack(PlayerContext ctxt, float? duration = null, Action onFadeComplete = null)
        {
            if (m_currentSoundtrackFader != null)
            {
                m_currentSoundtrackFader.Stop();
                m_currentSoundtrackFader = null;
            }

            var fadeDuration = duration ?? FadeDuration;

            var _fader = m_currentSoundtrackFader;

            m_currentSoundtrackFader = Fader.FadeOut(ctxt.Player, TimeSpan.FromSeconds(fadeDuration),
                onComplete: () =>
                 {
                    lock (SoundtrackLockObject)
                    {
                        if (_fader == m_currentSoundtrackFader)
                        {
                            m_currentSoundtrackFader = null;
                        }
                    }

                    if (onFadeComplete != null)
                    {
                        onFadeComplete();
                    }
                });

        }

        public void PlayAudioContent(string instanceId, LocalizedAudioContent audioContent, AudioContentRoute route, Action<Action> beforeOpen, Action onComplete)
        {
            if (IsPaused)
            {
                m_onUnpause[instanceId] = () => { PlayAudioContent(instanceId, audioContent, route, beforeOpen, onComplete); };
                return;
            }

            if (audioContent == null ||
                audioContent.MediaItem == null)
            {
                m_logger.Warning("Playable did not contain audio content!");

                if (onComplete != null)
                {
                    onComplete();
                }

                return;
            }

            var path = WebServices.Instance.MediaDownloadManager.GetPathForItem(audioContent.MediaItem.Url);
            var player = m_channel.CreatePlayer(new Uri(path));
            player.Loop = audioContent.Loop;
            player.Volume = audioContent.Volume;

            var ctxt = new PlayerContext(instanceId, player, audioContent);

            lock (m_playablePlayers)
            {
                m_playablePlayers.Add(instanceId, ctxt);
            }

            // Set up values
            Action play = () =>
            {
                switch (route)
                {
                    case AudioContentRoute.Soundtrack:
                        {
                        // FIX:: SOUNDTRACK LOCK OBJECT.
                        lock (SoundtrackLockObject)
                            {
                                bool fadeIn = false;

                                if (CurrentSoundtrackPlayer != null && m_isPlayingSoundtrack)
                                {
                                    fadeIn = true;

                                // Main thing we're trying to do here is make sure we don't stack a bunch
                                // of previously activated sounds if they weren't deactivated
                                if (m_currentSoundtrackPlayTime.HasValue &&
                                    (DateTime.Now - m_currentSoundtrackPlayTime.Value).TotalSeconds < FadeDuration / 2)
                                    {
                                        CurrentSoundtrackPlayer.Stop();
                                    }
                                    else
                                    {
                                        Fader.FadeOut(CurrentSoundtrackPlayer.Player, TimeSpan.FromSeconds(FadeDuration));
                                    }
                                }

                                m_soundtrackPlayers.Add(ctxt);

                                if (m_isPlayingSoundtrack)
                                {
                                    m_currentSoundtrackPlayTime = DateTime.Now;

                                    if (fadeIn)
                                    {
                                        FadeInSoundtrack(ctxt, FadeDuration, onComplete);
                                    }
                                    else
                                    {
                                        player.Play((success) =>
                                        {
                                            StopPlaying(instanceId);

                                            onComplete();
                                        });
                                    }
                                }
                            }
                            break;
                        }
                    case AudioContentRoute.Narrator:
                        {
                            CurrentNarratorPlayer = player;

                        // Optionally duck the other channels
                        DuckExcept(ctxt);

                            player.Play((success) =>
                            {
                                CurrentNarratorPlayer = null;

                                Unduck();

                                StopPlaying(instanceId);

                                onComplete();
                            });

                            break;
                        }
                    case AudioContentRoute.Ambient:
                    default:
                        {
                            player.Play((success) =>
                            {
                                StopPlaying(instanceId);

                                onComplete();
                            });

                            break;
                        };
                }
            };

            if (beforeOpen != null)
            {
                beforeOpen(play);
            }
            else
            {
                play();
            }
        }

        public void PlayAudioContent(string instanceId, LocalizedAudioContent audioContent, AudioContentRoute route, Action onComplete)
        {
            PlayAudioContent(instanceId, audioContent, route, null, onComplete);
        }

        public bool StopPlaying(string instanceId, bool interrupt = false)
        {
            PlayerContext ctxt = null;

            lock (m_onUnpause)
            {
                m_onUnpause.Remove(instanceId);
            }

            if (m_playablePlayers.TryGetValue(instanceId, out ctxt))
            {
                var player = ctxt.Player;

                m_playablePlayers.Remove(instanceId);

                // FIX:: SOUNDTRACK LOCK OBJECT.
                lock (SoundtrackLockObject)
                {
                    if (m_soundtrackPlayers.Contains(ctxt))
                    {
                        var origSoundtrackPlayer = CurrentSoundtrackPlayer;

                        m_soundtrackPlayers.Remove(ctxt);

                        if (m_isPlayingSoundtrack)
                        {
                            if (origSoundtrackPlayer == ctxt)
                            {
                                // If we just removed the currently playing
                                // soundtrack player
                                Fader.FadeOut(origSoundtrackPlayer.Player, TimeSpan.FromSeconds(FadeDuration),
                                    () =>
                                    {
                                        origSoundtrackPlayer.Player.Dispose();
                                    });

                                if (CurrentSoundtrackPlayer != null)
                                {
                                    FadeInSoundtrack(CurrentSoundtrackPlayer);
                                }
                            }
                            else
                            {
                                player.Dispose();
                            }
                        }
                        else
                        {
                            player.Dispose();
                        }
                    }
                    else
                    {
                        if (IsPaused || interrupt || player != CurrentNarratorPlayer)
                        {
                            // Don't immediately dispose if this is the current narrator player: let
                            // the narrator audio complete on its own.
                            player.Dispose();
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

            }

            return true;
        }

        public void StartSoundtrack()
        {

            // FIX:: SOUNDTRACK LOCK OBJECT.
            lock (SoundtrackLockObject)
            {
                m_isPlayingSoundtrack = true;

                if (CurrentSoundtrackPlayer != null)
                {
                    m_currentSoundtrackPlayTime = DateTime.Now;

                    CurrentSoundtrackPlayer.Play();
                }
            }
        }

        void PauseSoundtrack(Action onPaused = null)
        {
            // FIX:: SOUNDTRACK LOCK OBJECT.
            lock (SoundtrackLockObject)
            {
                if (m_isPlayingSoundtrack)
                {
                    m_isPlayingSoundtrack = false;

                    if (CurrentSoundtrackPlayer != null)
                    {
                        FadeOutSoundtrack(CurrentSoundtrackPlayer, FadeDuration / 3, onPaused);
                    }
                }
            }
        }

        void ResumeSoundtrack()
        {
            // FIX:: SOUNDTRACK LOCK OBJECT.
            lock (SoundtrackLockObject)
            {
                if (!m_isPlayingSoundtrack)
                {
                    m_isPlayingSoundtrack = true;

                    if (CurrentSoundtrackPlayer != null)
                    {
                        FadeInSoundtrack(CurrentSoundtrackPlayer, FadeDuration / 3);
                    }
                }
            }
        }
    }

}