// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.Webcam
{
    public abstract class CameraProvider : MonoBehaviour
    {
        public bool CameraIsStopped;
        protected Quaternion OriginalRotation;
        protected DeviceOrientation PreviousOrientation;
        protected Texture LivePreviewCoreTexture;

        public Dictionary<GameObject, Action<Texture>> PreviewTextures;
        public abstract void TakePicture(int width, int height, Action<Texture2D> onImageAction);
        public abstract void Focus();
        public abstract void OrientateCamera();
        public abstract Color32[] GetPixels32();

        public virtual void StartCameraPreview()
        {
            CameraIsStopped = false;
        }

        public virtual void StopCameraPreview()
        {
            CameraIsStopped = true;
        }

        /// <summary>
        /// In a camera provider this method should set the LivePreviewCoreTexture.
        /// </summary>
        public abstract void SetCameraPreviewTexture();


        public void UnsubscribeFromLivePreviews(GameObject obj)
        {
            if (PreviewTextures.ContainsKey(obj))
            {
                PreviewTextures.Remove(obj);
            }
        }

        public void SubscribeToPreview(GameObject obj, Action<Texture> tex)
        {
            // This collection is useless for now, but the idea is to have a ref back to the object later.
            if (!PreviewTextures.ContainsKey(obj))
            {
                PreviewTextures.Add(obj, tex);
            }

            StartCoroutine(WaitForCoreTexture(tex));
        }


        public IEnumerator WaitForCoreTexture(Action<Texture> tex)
        {
            while (LivePreviewCoreTexture == null)
            {
                yield return new WaitForEndOfFrame();
            }

            tex(LivePreviewCoreTexture);
        }

        protected void Awake()
        {

            PreviewTextures = new Dictionary<GameObject, Action<Texture>>();

            //if (!LivePreview)
            //{
            //    return;
            //}

            //var oRot = LivePreview.transform.rotation;
            //_originalRotation = new Quaternion(oRot.x, oRot.y, oRot.z, oRot.w);
        }

        protected  void Update()
        {
            if (CameraIsStopped)
            {
                return;
            }

            PreviousOrientation = Input.deviceOrientation;

            OrientateCamera();

        }

    }
}
