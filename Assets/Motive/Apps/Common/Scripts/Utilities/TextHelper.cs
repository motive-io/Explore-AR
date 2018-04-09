// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine.UI;

namespace Motive.Unity.Utilities
{
    public static class TextHelper
    {
        public static bool CheckOverflow(Text textBox)
        {
            var prefHeight = LayoutUtility.GetPreferredHeight(textBox.rectTransform);
            var prefWidth = LayoutUtility.GetPreferredWidth(textBox.rectTransform);

            return prefHeight > textBox.rectTransform.rect.height ||
                    prefWidth > textBox.rectTransform.rect.width;
        }

        public static string StrikeThrough(string s)
        {
            string strikethrough = "";
            foreach (char c in s)
            {
                strikethrough = strikethrough + c + '\u0336';
            }
            return strikethrough;
        }
    }

}