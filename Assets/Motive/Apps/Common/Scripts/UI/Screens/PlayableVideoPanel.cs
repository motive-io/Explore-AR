// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine.Events;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays playable video.
    /// </summary>
    public class PlayableVideoPanel : PlayableMediaPanel
    {
        public UnityEvent PlaybackCompleted;

        protected override void Awake()
        {
            var component = GetComponent<VideoContentPlayerComponent>();

            if (component)
            {
                component.PlaybackCompleted.AddListener(() =>
                {
                    if (PlaybackCompleted != null)
                    {
                        PlaybackCompleted.Invoke();
                    }
                });
            }

            base.Awake();
        }
    }

}