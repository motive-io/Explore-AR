// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Apps;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays and manages a set of debug settings for Motive apps.
    /// </summary>
    public class DebugSettingsPanel : Panel
    {
        public Toggle UseDevCatalogs;
        public Text AppVersion;

        public PanelLink ResetDialog;

        public void Reset()
        {
            if (ResetDialog)
            {
                var panel = ResetDialog.GetPanel<OptionsDialogPanel>();

                if (panel)
                {
                    panel.Show(new string[] { "OK", "Cancel" },
                        (opt) =>
                        {
                            if (opt == "OK")
                            {
                                Back();

                                AppManager.Instance.Reset();
                            }
                        });
                }
            }
            else
            {
                Back();

                AppManager.Instance.Reset();
            }
        }

        public override void DidPush()
        {
            if (UseDevCatalogs)
            {
                UseDevCatalogs.isOn = PlayerPrefs.GetInt("Debug_UseDevCatalogs") != 0;
            }

            if (AppVersion)
            {
                AppVersion.text = "v " + Application.version;
            }

            base.DidPush();
        }

        public void ToggleUseDevCatalogs(bool value)
        {
            PlayerPrefs.SetInt("Debug_UseDevCatalogs", value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void Reload()
        {
            Back();

            AppManager.Instance.Reload();
        }
    }

}