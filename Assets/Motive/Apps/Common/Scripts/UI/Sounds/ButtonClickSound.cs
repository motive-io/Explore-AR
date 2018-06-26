// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Utility component for adding a "click" sound to buttons.
    /// </summary>
    public class ButtonClickSound : MonoBehaviour
    {
        Button m_button;
        AudioSource m_audioSource;

        public AudioClip Sound;

        void Awake()
        {
            m_button = gameObject.GetComponent<Button>();
            m_audioSource = gameObject.AddComponent<AudioSource>();

            if (m_button)
            {
                m_button.onClick.AddListener(PlaySound);
            }
        }

        void PlaySound()
        {
            m_audioSource.PlayOneShot(Sound);
        }
    }

}