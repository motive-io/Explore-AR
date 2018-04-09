// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.Globalization
{
    /// <summary>
    /// Class managing UI text localization. Language specific strings shall be saved following this
    /// folder structure:
    ///
    ///     Resources/localization/English.txt 
    ///     Resources/localization/Italian.txt 
    ///     Resources/localization/Japanese.txt
    ///
    /// ... and so on, where the file name (and consequently the resource name) is the string version of
    /// the SystemLanguage enumeration.
    ///
    /// The file format is as follows:
    ///
    ///     key=value
    ///
    /// A TAB character is also accepted as key/value separator. 
    /// In the value you can use the standard /// notation for newline: \n
    /// </summary>

    public abstract class LocalizeBase : MonoBehaviour
    {

        public string localizationKey;


        public abstract void UpdateLocale();

        protected virtual void Start()
        {
            if (!Locale.currentLanguageHasBeenSet)
            {
                Locale.SetCurrentLanguage(Locale.PlayerLanguage);
            }
            UpdateLocale();
        }

        public static string GetLocalizedString(string key, string defaultValue = "")
        {
            if (Locale.CurrentLanguageStrings.ContainsKey(key))
                return Locale.CurrentLanguageStrings[key];
            else
                return defaultValue;
        }

        public static void SetText(Text text, string key, string defaultValue = "")
        {
            if (text)
            {
                text.text = GetLocalizedString(key, defaultValue);
            }
        }

        public static void SetUppercaseText(Text text, string key, string defaultValue = "")
        {
            if (text)
            {
                text.text = GetLocalizedString(key, defaultValue).ToUpper();
            }
        }
    }
}