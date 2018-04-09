// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using UnityEngine;
using UnityEngine.UI;
//using Mapbox.Scripts.Utilities;

namespace Motive.Unity.Webcam
{
    /// <summary>
    /// A provider of the 
    /// </summary>
    public class UnityWebcamProvider
#if MOTIVE_USES_CAMERA
        : CameraProvider
#endif
    {
#if MOTIVE_USES_CAMERA
        public WebCamTexture Texture;

        public WebCamTexture CreateWebCameraTexture()
        {
            var devices = WebCamTexture.devices;
            string backCamName = null;
            WebCamTexture camTex = null;
            
            for (int i = 0; i < devices.Length; i++)
            {

#if UNITY_EDITOR
                backCamName = devices[i].name;
#else
            if (!devices[i].isFrontFacing)
            {
                backCamName = devices[i].name;
            }
#endif
            }

            if (backCamName == null)
            {
                return null;
            }

            camTex = new WebCamTexture(backCamName);
            camTex.Play();

            return camTex;
        }


        public override void TakePicture(int width, int height, Action<Texture2D> onImageAction)
        {
            if (!Texture.isPlaying)
            {
                Texture.Play();
            }

			width = Math.Min(Texture.width, width);
			height = Math.Min(Texture.height, height);

			// Scale to less-restricted dimension
			var sw = (double)Texture.width / (double)width;
			var sh = (double)Texture.height / (double)height;

			//var aspect = (double)width / (double)height;
			//var scale = Math.Min(sw, sh);

			Texture2D tex;

			if (sw > sh)
			{
				// Take full height, relative width
				tex = new Texture2D((int)(Texture.width * sh / sw), Texture.height, TextureFormat.ARGB32, false);

				var pix = Texture.GetPixels((Texture.width - tex.width) / 2, 0, tex.width, tex.height);

				tex.SetPixels(pix);
			}
			else
			{
				// Take full width, relative height
				tex = new Texture2D(Texture.width, (int)(Texture.height * sw / sh), TextureFormat.ARGB32, false);

				var pix = Texture.GetPixels(0, Texture.height - tex.height, tex.width, tex.height);

				tex.SetPixels(pix);
			}

#if MAPBOX_TODO
			TextureScale.Bilinear(tex, width, height);
#endif

			// Currently default to top-middle
            switch (Input.deviceOrientation)
            {
                case DeviceOrientation.Portrait:
                case DeviceOrientation.PortraitUpsideDown:
                case DeviceOrientation.FaceUp:
                    tex = CameraManager.RotateMByNImage(tex);
                    break;
                case DeviceOrientation.LandscapeRight:
                    //Two rotations (180)
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.RotateMByNImage(tex);
                    break;
                case DeviceOrientation.FaceDown:
                case DeviceOrientation.Unknown:
                case DeviceOrientation.LandscapeLeft:
                    break;
                default:
                    break;
            }

            onImageAction(tex);            
        }

        public override Color32[] GetPixels32()
        {
            return Texture.GetPixels32();
        }

        public override void Focus()
        {
            // Cant focus the 
        }

        public override void SetCameraPreviewTexture()
        {
            LivePreviewCoreTexture = Texture;
        }

        public override void OrientateCamera()
        {
            // Do Nothing.
        }

        public override void StartCameraPreview()
        {
            if (!Texture)
            {
                Texture = CreateWebCameraTexture();
            }

            Texture.Play();
            
            SetCameraPreviewTexture();

            base.StartCameraPreview();

        }

        public override void StopCameraPreview()
        {
            if (Texture)
            {
                Texture.Stop();
            }

            base.StopCameraPreview();
        }
#endif
    }
}