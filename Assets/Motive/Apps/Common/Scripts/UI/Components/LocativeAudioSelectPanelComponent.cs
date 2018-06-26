// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays locative audio information for a locative audio map annotation.
    /// </summary>
    public class LocativeAudioSelectPanelComponent : PanelComponent<LocativeAudioAnnotation>
    {
        public Text MinRange;
        public Text MaxRange;
        public Text Sound;
        public Text Attenuation;
        public Text SpatialType;

        public override void Populate(LocativeAudioAnnotation obj)
        {
            if (obj.LocativeAudio.DistanceRange != null)
            {
                if (MinRange)
                {
                    MinRange.text = obj.LocativeAudio.DistanceRange.Min.GetValueOrDefault().ToString();
                }

                if (MaxRange && obj.LocativeAudio.DistanceRange.Max.HasValue)
                {
                    MaxRange.text = obj.LocativeAudio.DistanceRange.Max.Value.ToString();
                }
            }

            if (obj.LocativeAudio.LocalizedAudioContent != null && Sound)
            {
                var url = new System.Uri(obj.LocativeAudio.LocalizedAudioContent.ContentUrl);
                var fileName = System.IO.Path.GetFileName(url.LocalPath);
                Sound.text = fileName;
            }

            if (Attenuation)
            {
                Attenuation.text = obj.LocativeAudio.AttenuationMode.ToString();
            }

            if (SpatialType)
            {
                SpatialType.text = obj.LocativeAudio.Is3D ? "3D" : "2D";
            }
        }
    }

}