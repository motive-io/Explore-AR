// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Component for playing back audio on a panel.
    /// </summary>
    public class AudioContentPlayerComponent : MediaContentComponent
    {
        AudioSubpanel m_subpanel;

        public UnityEvent PlaybackCompleted;

        protected override void Awake()
        {
            base.Awake();

            m_subpanel = GetComponentInChildren<AudioSubpanel>();
            m_subpanel.PlaybackCompleted.AddListener(m_subpanel_PlaybackCompleted);

            if (PlaybackCompleted == null)
            {
                PlaybackCompleted = new UnityEvent();
            }
        }

        public override void Populate(Motive.Core.Models.MediaContent obj)
        {
            m_subpanel.Play(obj.MediaItem);

            base.Populate(obj);
        }

        void m_subpanel_PlaybackCompleted()
        {
            if (PlaybackCompleted != null)
            {
                PlaybackCompleted.Invoke();
            }
        }

        public override void DidHide()
        {
            m_subpanel.Close();

            base.DidHide();
        }
    }
}