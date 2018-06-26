// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.Globalization
{
    public class LanguageSelector : MonoBehaviour
    {
        public Dropdown dropdown;

        public void SetLanguage()
        {
            int value = dropdown.value;

            switch (value)
            {
                //case "English":
                case 0:
                    Locale.SetCurrentLanguage(SystemLanguage.English);
                    break;
                //case "Français":
                case 1:
                    Locale.SetCurrentLanguage(SystemLanguage.French);
                    break;
                default:
                    Locale.SetCurrentLanguage(SystemLanguage.English);
                    break;
            }

        }

        public void Start()
        {
            dropdown = GetComponent<Dropdown>();

            switch (Locale.PlayerLanguage.ToString())
            {
                case "English":
                    dropdown.value = 0;
                    break;
                case "French":
                    dropdown.value = 1;
                    break;
                default:
                    dropdown.value = 0;
                    break;
            }
        }
    }
}