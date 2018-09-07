using Motive.Unity.Timing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.UI
{
    public class ShowHideTimer : MonoBehaviour
    {
        public float DefaultDuration = 5f;

        public void Show()
        {
            SetVisible(DefaultDuration, true);
        }

        public void Hide()
        {
            SetVisible(DefaultDuration, false);
        }

        public void Toggle()
        {
            SetVisible(DefaultDuration, !gameObject.activeSelf);
        }

        public void SetVisible(float duration, bool startState)
        {
            gameObject.SetActive(startState);

            UnityTimer.Call(duration, () =>
            {
                gameObject.SetActive(!startState);
            });
        }
    }
}
