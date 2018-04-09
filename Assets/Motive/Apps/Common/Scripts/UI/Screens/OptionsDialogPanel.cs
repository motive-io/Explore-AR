// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using System;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    public class OptionsDialogPanelData
    {
        public string[] Options { get; set; }
        public Action<string> OnSelect { get; set; }

        public string Title { get; set; }
    }

    /// <summary>
    /// Displays a set of options for the user.
    /// </summary>
    public class OptionsDialogPanel : Panel<OptionsDialogPanelData>
    {
        public Text Text;
        void OnSelect(int optIdx)
        {
            Back();

            if (Data.OnSelect != null)
            {
                string optVal = null;

                if (Data.Options != null && Data.Options.Length > optIdx)
                {
                    optVal = Data.Options[optIdx];
                }

                Data.OnSelect(optVal);
            }
        }

        public override void Populate(OptionsDialogPanelData data)
        {
            if (Text)
            {
                Text.text = data.Title;
            }

            base.Populate(data);
        }

        public void Option1()
        {
            OnSelect(0);
        }

        public void Option2()
        {
            OnSelect(1);
        }

        public void Option3()
        {
            OnSelect(2);
        }

        public void Option4()
        {
            OnSelect(3);
        }

        public void Confirm(Action onConfirmed, Action onCancel = null)
        {
            Show("OK", "CANCEL", (opt) =>
                {
                    if (opt == "OK")
                    {
                        if (onConfirmed != null)
                        {
                            onConfirmed();
                        }
                    }
                    else if (opt == "CANCEL")
                    {
                        if (onCancel != null)
                        {
                            onCancel();
                        }
                    }
                });
        }

        public void Show(string okOption, string cancelOption, Action<string> onSelect)
        {
            Show(new string[] { okOption, cancelOption }, onSelect);
        }

        public void Show(string[] options, Action<string> onSelect)
        {
            var data = new OptionsDialogPanelData
            {
                Options = options,
                OnSelect = onSelect
            };

            PanelManager.Instance.Push(this, data);
        }

        public static void Show(string title, string[] options, Action<string> onSelect)
        {
            var data = new OptionsDialogPanelData
            {
                Title = title,
                Options = options,
                OnSelect = onSelect
            };

            PanelManager.Instance.Push<OptionsDialogPanel>(data);
        }
    }

}