// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI;
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using UnityEngine;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// An annotation object that displays a custom image from the Motive server.
    /// </summary>
    public class CustomImageAnnotation : AnnotationGameObject
    {
        public bool AspectFit;
        public Renderer ImageRenderer;
        public Transform LayoutElement;

        public void LoadMediaElement(MediaElement element)
        {
            if (element.MediaUrl != null)
            {
                if (element.Layout != null)
                {
                    LayoutHelper.Apply(LayoutElement ?? this.transform, element.Layout);
                }

                ImageLoader.LoadImageOnThread(element.MediaUrl, (ImageRenderer != null) ? ImageRenderer.gameObject : this.gameObject, AspectFit);

				if (element.Color != null)
				{
					var renderer = ImageRenderer ?? (this.gameObject.GetComponent<Renderer>());

					if (renderer)
					{
						renderer.material.color = ColorHelper.ToUnityColor(element.Color);
					}
				}
            }
        }

        public void LoadImage(string imageUrl)
        {
            if (imageUrl != null)
            {
                ImageLoader.LoadImageOnThread(imageUrl, (ImageRenderer != null) ? ImageRenderer.gameObject : this.gameObject, AspectFit);
            }
        }
    }
}
