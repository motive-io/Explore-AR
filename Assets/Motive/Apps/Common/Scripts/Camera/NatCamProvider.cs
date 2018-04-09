// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using JetBrains.Annotations;
using Motive.Unity.Webcam;
using UnityEngine;

#if MOTIVE_NATCAM
using NatCamU.Core;
using NatCamU.Core.UI;
public class NatCamProvider : CameraProvider
{

    public override void Photograph(Action<Texture2D> onImageAction)
    {

        try
        {
            NatCam.Camera.SetPhotoResolution(512, 512);

            NatCam.CapturePhoto((image, orientation) =>
            {
                onImageAction(OrientatePhoto(image, Input.deviceOrientation));
            });
        }
        catch (Exception e)
        {

            Debug.Log("Failed to take photo, trying a low res version");
            Debug.Log(e.Message);

            try
            {
                NatCam.Camera.SetPhotoResolution(ResolutionPreset.LowestResolution);

                NatCam.CapturePhoto((image, orientation) =>
                {
                    onImageAction(OrientatePhoto(image, Input.deviceOrientation));
                });
            }
            catch (Exception lowResAttemptException)
            {
                Debug.Log("Failed to take photo");
                Debug.Log(lowResAttemptException.Message);
            }
           
        }
        
    }


    public Texture2D OrientatePhoto(Texture2D tex, DeviceOrientation orientation)
    {
        
        if (Application.platform == RuntimePlatform.Android)
        {
            switch (orientation)
            {
                case DeviceOrientation.Unknown:
                    break;
                case DeviceOrientation.Portrait:
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.MirrorImage(tex);
                    break;
                case DeviceOrientation.PortraitUpsideDown:
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.MirrorImage(tex);
                    break;
                case DeviceOrientation.LandscapeLeft:
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.MirrorImage(tex);
                    break;
                case DeviceOrientation.LandscapeRight:
                    tex = CameraManager.MirrorImage(tex);
                    break;
                case DeviceOrientation.FaceUp:
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.MirrorImage(tex);
                    break;
                case DeviceOrientation.FaceDown:
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.MirrorImage(tex);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("orientation", orientation, null);
            }
        }
        if (Application.platform == RuntimePlatform.IPhonePlayer) // This is the same as android. Left in case we need to change something later.
        {
            switch (orientation)
            {
                case DeviceOrientation.Unknown:
                    break;
                case DeviceOrientation.Portrait:
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.MirrorImage(tex);
                    break;
                case DeviceOrientation.PortraitUpsideDown:
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.MirrorImage(tex);
                    break;
                case DeviceOrientation.LandscapeLeft:
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.MirrorImage(tex);
                    break;
                case DeviceOrientation.LandscapeRight:
                    tex = CameraManager.MirrorImage(tex);
                    break;
                case DeviceOrientation.FaceUp:
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.MirrorImage(tex);
                    break;
                case DeviceOrientation.FaceDown:
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.RotateMByNImage(tex);
                    tex = CameraManager.MirrorImage(tex);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("orientation", orientation, null);
            }
        }

        return tex;
    }


    public override void Focus()
    {
        NatCam.Camera.FocusMode = FocusMode.AutoFocus;
    }

    public override void OrientateCamera()
    {
        // Do nothing.
    }

    public override void StartCameraPreview()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            NatCam.Play(DeviceCamera.RearCamera);
        }
        else
        {
            NatCam.Play(DeviceCamera.FrontCamera);
        }

        SetCameraPreviewTexture();
    }

    public override void StopCameraPreview()
    {
        NatCam.Pause();
    }

    public override void SetCameraPreviewTexture()
    {
        NatCam.OnStart += () =>
        {
            //LivePreview.texture = NatCam.Preview;
            _livePreviewCoreTexture =  NatCam.Preview;
        };
    }


}
#endif