// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.Globalization
{
    [RequireComponent(typeof(Text))]
    public class Localize : LocalizeBase
    {
        public Text Text;
        public string Default;

        public override void UpdateLocale()
        {
            if (!Text) return;

            var val = Localize.GetLocalizedString(this.localizationKey, Default);

            Text.text = val;
        }

        protected override void Start()
        {
            if (!Text)
            {
                Text = GetComponent<Text>();
            }

            if (Text && string.IsNullOrEmpty(Default))
            {
                Default = Text.text;
            }

            base.Start();
        }
    } 
}
