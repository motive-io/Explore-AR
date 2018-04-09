// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays AR Task information.
    /// </summary>
    public class ARTaskInfoComponent : PanelComponent<ARTaskDriver>
    {
        public Text Title;

        public override void DidShow(ARTaskDriver driver)
        {
            driver.Updated += driver_Updated;

            base.DidShow(driver);
        }

        public override void Populate(ARTaskDriver driver)
        {
            if (Title)
            {
                Title.text = driver.Task.Title;
            }

            base.Populate(driver);
        }

        void driver_Updated(object sender, System.EventArgs e)
        {
            Populate(Data);
        }

        public override void DidHide()
        {
            if (Data != null)
            {
                Data.Updated -= driver_Updated;

                //ARViewManager.Instance.SetGuide(null);
            }
        }

        public void Action()
        {
            //UIModeManager.Instance.SetARMode();
        }
    }
}