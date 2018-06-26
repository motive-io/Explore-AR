// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.UI.Framework;
using UnityEngine.UI;
using Motive.AR.LocationServices;
using Motive.Unity.AR;
using Motive.Unity.Utilities;

namespace Motive.Unity.UI
{
    public class ARSelectPanel<T> : Panel<T> where T : LocationARWorldObject
    {
        public Text TitleText;
        public Text SubtitleText;
        public Text DistanceText;

        public virtual string Title
        {
            get
            {
                return (Data.LocationTaskDriver != null) ?
                    Data.LocationTaskDriver.Task.Title : Data.Location.Name;
            }
        }

        public virtual string Subtitle
        {
            get
            {
                return (Data.LocationTaskDriver != null) ? Data.Location.Name : null;
            }
        }

        public override void Populate(T data)
        {
            if (TitleText)
            {
                TitleText.text = Title;
            }

            if (SubtitleText)
            {
                SubtitleText.text = Subtitle;
            }

            base.Populate(data);
        }

        void Update()
        {
            if (DistanceText)
            {
                var d = ForegroundPositionService.Instance.Position.GetDistanceFrom(Data.Coordinates);

                DistanceText.text = StringFormatter.FormatDistance(d);
            }
        }
    }

}