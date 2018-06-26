// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Motive.Unity.Globalization
{
    public static class Locale
    {
        const string STR_LOCALIZATION_KEY = "locale";
        const string STR_LOCALIZATION_PREFIX = "localization/";
        static string currentLanguage;
        //static bool currentLanguageFileHasBeenFound = false;
        public static bool currentLanguageHasBeenSet = false;
        public static Dictionary<string, string> CurrentLanguageStrings = new Dictionary<string, string>();
        static TextAsset currentLocalizationText;

        static Dictionary<SystemLanguage, string> m_cultureCodes = new Dictionary<SystemLanguage, string>
    {
        { SystemLanguage.English, "en" },
        { SystemLanguage.French, "fr" },
    };

        public static void SetCurrentLanguage(SystemLanguage language)
        {
            Locale.PlayerLanguage = language;

            LoadLanguageStrings(language.ToString());

            Localize[] allTexts = UnityEngine.Object.FindObjectsOfType<Localize>();

            for (int i = 0; i < allTexts.Length; i++)
            {
                allTexts[i].UpdateLocale();
            }

            SetMotiveLanguage(language);

            currentLanguageHasBeenSet = true;
        }

        static void SetMotiveLanguage(SystemLanguage language)
        {
            string cc;

            if (m_cultureCodes.TryGetValue(language, out cc))
            {
                var ci = new CultureInfo(cc);

                Motive.Core.Globalization.LocalizationSettings.Instance.OverrideCulture(ci);
            }
        }

        static void LoadLanguageStrings(string value)
        {
            if (value != null && value.Trim() != string.Empty)
            {
                currentLanguage = value;
                currentLocalizationText = Resources.Load(STR_LOCALIZATION_PREFIX + currentLanguage, typeof(TextAsset)) as TextAsset;
                if (currentLocalizationText == null)
                {
                    Debug.LogWarningFormat("Missing locale '{0}', loading English.", currentLanguage);

                    if (LanguageSettings.Instance)
                    {
                        currentLanguage = LanguageSettings.Instance.DefaultLanguage.ToString();
                    }
                    else
                    {
                        currentLanguage = SystemLanguage.English.ToString();
                    }

                    currentLocalizationText = Resources.Load(STR_LOCALIZATION_PREFIX + currentLanguage, typeof(TextAsset)) as TextAsset;
                }
                if (currentLocalizationText != null)
                {
                    //currentLanguageFileHasBeenFound = true;
                    // One line per key pair
                    string[] lines = currentLocalizationText.text.Split(new string[] { "\r\n", "\n\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
                    CurrentLanguageStrings.Clear();
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string[] pairs = lines[i].Split(new char[] { '\t', '=' }, 2);
                        if (pairs.Length == 2)
                        {
                            CurrentLanguageStrings.Add(pairs[0].Trim(), pairs[1].Trim());
                        }
                    }
                }
                else
                {
                    Debug.LogWarningFormat("Locale Language '{0}' not found!", currentLanguage);
                }
            }
        }

        public static bool CurrentLanguageHasBeenSet
        {
            get
            {
                return currentLanguageHasBeenSet;
            }
        }

        /// <summary>
        /// The player language. If not set in PlayerPrefs then returns Application.systemLanguage
        /// </summary>
        public static SystemLanguage PlayerLanguage
        {
            get
            {
                return (SystemLanguage)PlayerPrefs.GetInt(STR_LOCALIZATION_KEY, (int)Application.systemLanguage);
            }
            set
            {
                PlayerPrefs.SetInt(STR_LOCALIZATION_KEY, (int)value);
                PlayerPrefs.Save();
            }
        }
    }

}