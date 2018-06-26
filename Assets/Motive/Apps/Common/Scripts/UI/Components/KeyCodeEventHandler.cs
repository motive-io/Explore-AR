using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Motive.Unity.UI
{
    public class KeyCodeEventHandler : MonoBehaviour
    {
        public bool AnyKey;
        public KeyCode KeyCode;

        public UnityEvent OnKeyDown;
        public UnityEvent OnKeyUp;

        private void Update()
        {
            if (AnyKey && Input.anyKeyDown || (Input.GetKeyDown(KeyCode)))
            {
                if (OnKeyDown != null)
                {
                    OnKeyDown.Invoke();
                }
            }

            if (Input.GetKeyUp(KeyCode))
            {
                if (OnKeyUp != null)
                {
                    OnKeyUp.Invoke();
                }
            }
        }
    }
}