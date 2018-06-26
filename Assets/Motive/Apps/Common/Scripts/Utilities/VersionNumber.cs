// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.Utilities
{
    /// <summary>
    /// Displays the app version number.
    /// </summary>
    public class VersionNumber : MonoBehaviour
    {
        public Text VersionText;

        void Awake()
        {
            if (!VersionText)
            {
                VersionText = GetComponent<Text>();
            }
        }

        void Start()
        {
            VersionText.text = "v " + Application.version;
        }
    }

}