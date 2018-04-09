// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.Utilities
{
    /// <summary>
    /// When invoked, applies the specified settings to text.
    /// </summary>
    public class TextUpdater : MonoBehaviour
    {
        public Text Text;
        public Color TextColor;
        public FontStyle FontStyle;
        public bool StrikeThrough;

        private void Awake()
        {
            if (!Text)
            {
                Text = GetComponent<Text>();
            }
        }

        public void UpdateText()
        {
            if (Text)
            {
                Text.color = TextColor;
                Text.fontStyle = FontStyle;

                if (StrikeThrough)
                {
                    Text.text = TextHelper.StrikeThrough(Text.text);
                }
            }
        }
    }

}