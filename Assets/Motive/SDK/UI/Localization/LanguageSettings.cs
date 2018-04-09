// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Utilities;
using UnityEngine;

namespace Motive.Unity.Globalization
{
    public class LanguageSettings : SingletonComponent<LanguageSettings>
    {
        public SystemLanguage DefaultLanguage = SystemLanguage.English;

        protected override void Start()
        {
            base.Start();

            Locale.SetCurrentLanguage(Locale.PlayerLanguage);
        }
    }

}