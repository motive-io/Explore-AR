// Copyright (c) 2018 RocketChicken Interactive Inc.
#if MPMP
using UnityEngine;
using System.Collections;
using monoflow;
using UnityEngine.UI;
using Motive.Core.Models;
using Motive.Core.Media;
using UnityEngine.Events;

public class MPMPVideoSubpanel : VideoSubpanel
{
    public MPMP Player;
    public RawImage PlayerSurface;

    bool m_isPlaying;

    public override bool IsPlaying
    {
        get { return m_isPlaying; }
    }

    public override float Position
    {
        get { return (float)Player.GetCurrentPosition(); }
    }

    public override float Duration
    {
        get { return (float)Player.GetDuration(); }
    }

    bool m_tryingToLoad;
    bool m_loadedVideo;
    bool m_startedPlay;
    bool m_waitingPlay;
    bool m_waitingSetTexture;

    void Awake()
    {
        if (PlaybackCompleted == null)
        {
            PlaybackCompleted = new UnityEvent();
        }

        if (ClipLoaded == null)
        {
            ClipLoaded = new UnityEvent();
        }
    }

    void Start()
    {
#if UNITY_ANDROID_x
        if (Application.isMobilePlatform)
        {
            var rect = PlayerSurface.uvRect;

            rect.y = 0;
            rect.height = 1;

            PlayerSurface.uvRect = rect;
        }
#endif
    }

    public override void Play()
    {
        m_isPlaying = true;

        if (!m_loadedVideo)
        {
            m_loadedVideo = false;
            m_tryingToLoad = true;
            m_startedPlay = false;
            m_waitingPlay = false;

            Player.Load();
        }
        else
        {
            Player.Play();

            if (PlayerSurface)
            {
                PlayerSurface.texture = Player.GetVideoTexture();
            }
        }
    }

    int m_txCt;

    // Update is called once per frame
    void Update()
    {
        if (!m_loadedVideo)
        {
            if (m_tryingToLoad)
            {
                if (Player.IsLoading())
                {
                    return;
                }
                else
                {
                    m_tryingToLoad = false;
                }
            }
            else
            {
                m_loadedVideo = !Player.IsLoading();

                if (m_loadedVideo)
                {
                    Player.Play();
                    m_waitingPlay = true;
                }
            }
        }
        else
        {
            if (m_waitingPlay && Player.IsPlaying())
            {
                m_waitingPlay = false;
                m_startedPlay = true;
                m_waitingSetTexture = true;
            }

            var tex = Player.GetVideoTexture();

            if (m_waitingSetTexture && tex != null && Player.GetCurrentPosition() > 0.01)
            {
                //PlayerSurface.texture = tex;
                m_waitingSetTexture = false;

                ClipLoaded.Invoke();
            }
        }

        if (m_startedPlay && !Player.IsPlaying())
        {
            m_startedPlay = false;
            m_isPlaying = false;

            PlaybackCompleted.Invoke();
        }
    }

    public override void Pause()
    {
        m_isPlaying = false;
        Player.Pause();
    }

    public override void Stop()
    {
        m_isPlaying = false;
        Player.Pause();
        Player.SeekTo(0);
    }

    public override void UpdatePosition(float pos)
    {
        Player.SeekTo(Duration * pos);
    }

    public void SetVideoPath(string path)
    {
        Player.videoPath = path;
    }

    public override float AspectRatio
    {
        get 
        {
            if (Player != null)
            {
                var tex = Player.GetVideoTexture();

                if (tex != null)
                {
                    return (float)tex.height / (float)tex.width;
                }
            }

            return 1f;
        }
    }

    public override void Play(string url)
    {
        m_isPlaying = true;

        m_loadedVideo = false;
        m_tryingToLoad = true;
        m_startedPlay = false;
        m_waitingPlay = false;

        if (url != null)
        {
            var videoUrl = WebServices.Instance.MediaDownloadManager.GetSystemUrl(url);

            Player.Load(videoUrl);

            if (PlayerSurface)
            {
                PlayerSurface.texture = Player.GetVideoTexture();
            }
        }
    }
}
#endif