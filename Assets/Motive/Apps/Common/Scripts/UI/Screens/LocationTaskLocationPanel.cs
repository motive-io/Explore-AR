// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Timing;
using Motive.Unity.Gaming;
using Motive.Unity.Maps;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Selected annotation panel for displaying location task annotations.
    /// </summary>
    public class LocationTaskLocationPanel : SelectedAnnotationPanel<MapAnnotation<LocationTaskDriver>>
    {
        LocationTaskDriver m_driver;

        public override void Populate(MapAnnotation<LocationTaskDriver> data)
        {
            base.Populate(data);

            if (data.Data != null)
            {
                m_driver = data.Data;

                string title;
                string description;
                string subtitle;

                var driverLocTitle = data.Data.Task.Title;
                title = (driverLocTitle != "") ? driverLocTitle : data.Location.Name;

                var character = data.Data.Task.Character;
                string charName = null;

                if (character != null)
                {
                    charName = character.Alias;
                }

                if (charName != null)
                {
                    subtitle = charName;

                    if (!string.IsNullOrEmpty(data.Location.Name))
                    {
                        subtitle += " at " + data.Location.Name;
                    }
                }
                else
                {
                    subtitle = data.Location.Name;
                }

                description = data.Data.Task.Description;

                if (InformationPane)
                {
                    InformationPane.SetActive(true);
                }

                if (Title)
                {
                    Title.text = title;
                }

                if (Subtitle)
                {
                    Subtitle.text = subtitle;
                }

                if (Description)
                {
                    Description.text = description;
                }

                ShowButton(m_driver.ShowActionButton, m_driver.ActionButtonText);

                if (m_driver.Task.ActionRange != null)
                {
                    SetButtonFence(data.Location, m_driver.Task.ActionRange,
                        m_driver.ActionButtonText,
                        m_driver.OutOfRangeActionButtonText);
                }
                else
                {
                    SetInRange(true);
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            if (TimeLeft)
            {
                if (m_driver != null && m_driver.TimeoutTimer != null)
                {
                    TimeLeft.gameObject.SetActive(true);

                    var dt = m_driver.TimeoutTimer.FireTime - ClockManager.Instance.Now;

                    TimeLeft.text = string.Format("{0:00}:{1:00}:{2:00}", dt.Hours, dt.Minutes, dt.Seconds);
                }
                else
                {
                    TimeLeft.gameObject.SetActive(false);
                }
            }
        }

        public override void Action()
        {
            base.Action();

            if (m_driver != null)
            {
                m_driver.Action(m_mapAnnotation.Location);
            }
        }
    }

}