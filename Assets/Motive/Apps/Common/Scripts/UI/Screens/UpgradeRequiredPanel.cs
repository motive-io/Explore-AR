// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.UI.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Used to prompt the user to upgrade their app.
    /// </summary>
    public class UpgradeRequiredPanel : Panel<UpgradeRequiredCommand>
    {
        public Text UpgradeNowMessage;
        public Text UpgradeButtonText;

        public override void Populate(UpgradeRequiredCommand data)
        {
            // upgrade text
            //
            if (data.LocalizedMessage != null && !string.IsNullOrEmpty(data.LocalizedMessage.Text)
                && UpgradeNowMessage)
            {
                UpgradeNowMessage.text = data.LocalizedMessage.Text;
            }

            // button text
            //
            if (data.LocalizedUpgradeButtonText != null && !string.IsNullOrEmpty(data.LocalizedUpgradeButtonText.Text)
                && UpgradeButtonText)
            {
                UpgradeButtonText.text = data.LocalizedUpgradeButtonText.Text;
            }

            base.Populate(data);
        }

        public void RedirectToStore()
        {
#if UNITY_ANDROID
            if (!string.IsNullOrEmpty(Data.AndroidStoreURL))
                Application.OpenURL(Data.AndroidStoreURL);
#elif UNITY_IPHONE
            if (!string.IsNullOrEmpty(Data.IOSStoreURL))
                Application.OpenURL(Data.IOSStoreURL);
#endif
        }

    }

}