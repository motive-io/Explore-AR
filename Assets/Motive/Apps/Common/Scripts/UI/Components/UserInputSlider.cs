// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Slider implementation that can distinguish between updates from the
    /// user sliding the item vs. updates that come from updating the slider
    /// value directly. Used in audio and video playback controls.
    /// </summary>
    public class UserInputSlider : Slider
    {
        public bool IsUserUpdate { get; private set; }

        public override void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
        {
            IsUserUpdate = true;

            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
        {
            IsUserUpdate = false;

            base.OnPointerUp(eventData);
        }
    }

}