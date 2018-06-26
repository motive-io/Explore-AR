// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.Webcam
{
	public class MeshWebCamPreview : WebCamPreview
	{
		public Renderer PreviewRenderer;

		void Awake()
		{
			var height = 2.0f;
			var width = height * Screen.width / Screen.height;
			transform.localScale = new Vector3(width, height, 0.1f);
		}

		protected override void ApplyAspectRatio (float aspectRatio)
		{
			// Aspect ratio w x h
			var height = 2f;

			// For mobile, fit it based on width--which after rotation
			// becomes the height
			if (Application.isMobilePlatform)
			{
				var width = height / aspectRatio;
				transform.localScale = new Vector3(height, width, 0.1f);
			}
			else
			{
				var width = height * aspectRatio;
				transform.localScale = new Vector3(width, height, 0.1f);
			}
		}

		protected override void SetTexture (Texture texture)
		{
			if (PreviewRenderer)
			{
				PreviewRenderer.material.mainTexture = texture;
			}
		}
	}
}