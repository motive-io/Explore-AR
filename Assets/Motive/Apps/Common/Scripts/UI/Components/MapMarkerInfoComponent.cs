// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.UI.Framework;
using Motive.Unity.Maps;
using Motive.Unity.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays map annotation information.
    /// </summary>
    public class MapMarkerInfoComponent : PanelComponent<MapAnnotation<LocationMarker>>
    {
        public GameObject InformationPane;

        public Text Title;
        public Text Subtitle;
        public Text InfoDescription;
        public RawImage InfoImage;

        public void SetTitle(string title)
        {
            if (Title)
            {
                Title.text = title;
            }
        }

        public void SetSubtitle(string subtitle)
        {
            if (Subtitle)
            {
                Subtitle.text = subtitle;
            }
        }

        public override void ClearData()
        {
            ObjectHelper.SetObjectActive(InformationPane, false);

            base.ClearData();
        }

        public override void DidShow(MapAnnotation<LocationMarker> data)
        {
            base.DidShow(data);

            if (data.Data != null && data.Data.Information != null)
            {
                SetTitle(data.Data.Information.Title ?? data.Location.Name);
                SetSubtitle(data.Data.Information.Subtitle);

                if (InformationPane)
                {
                    InformationPane.SetActive(true);

                    if (InfoDescription)
                    {
                        InfoDescription.text = data.Data.Information.Description;
                    }

                    if (InfoImage)
                    {
                        ImageLoader.LoadImageOnMainThread(data.Data.Information.ImageUrl, InfoImage);
                    }
                }
            }
            else
            {
                if (InformationPane)
                {
                    InformationPane.SetActive(false);
                }
            }
        }
    }
}