// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.Webcam
{
    public class UIWebCamPreview : WebCamPreview
    {
        public RawImage PreviewImage;

        protected override void ApplyAspectRatio(float aspectRatio)
        {
            if (PreviewImage)
            {
                var fitter = PreviewImage.GetComponent<AspectRatioFitter>();

                if (fitter)
                {
                    fitter.aspectRatio = aspectRatio;
                }
            }
        }

        protected override void SetTexture(Texture texture)
        {
            if (PreviewImage)
            {
                PreviewImage.texture = texture;
            }
        }
    }
}
