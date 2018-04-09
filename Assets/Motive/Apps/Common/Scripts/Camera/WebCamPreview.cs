// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using UnityEngine;

namespace Motive.Unity.Webcam
{
    public abstract class WebCamPreview : MonoBehaviour
    {
        /*
        public RawImage PreviewImage;
        public Renderer PreviewRenderer;
        public bool AdjustAspectScale;
        public GameObject RotateGameObject;*/

        bool m_isStarted;
        private bool m_appliedLayout;

        public bool Rotate;

        public Texture Texture { get; private set; }

        public float AspectRatio { get; private set; }

        protected abstract void SetTexture(Texture texture);

        protected abstract void ApplyAspectRatio(float aspectRatio);

        protected virtual void ApplyLayout(Texture texture)
        {
            AspectRatio = (float)Texture.width / (float)Texture.height;

            ApplyAspectRatio(AspectRatio);

            if (Rotate)
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                        {
                            var scale = transform.localScale;
                            transform.localScale = new Vector3(-scale.x, -scale.y, scale.z);
                            transform.Rotate(new Vector3(0, 0, 90f));
                            //AspectRatio = (float)Texture.height / (float)Texture.width;
                            break;
                        }
                    case RuntimePlatform.IPhonePlayer:
                        {
                            var scale = transform.localScale;
                            transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
                            transform.Rotate(new Vector3(0, 0, 90f));
                            //AspectRatio = (float)Texture.height / (float)Texture.width;
                            break;
                        }
                }
            }
        }

        void Update()
        {
            if (m_appliedLayout || !Texture || Texture.height < 100 || Texture.width < 100)
            {
                return;
            }

            ApplyLayout(Texture);

            m_appliedLayout = true;

            /**
            var videoRatio = (float)m_texture.width / (float)m_texture.height;

            if (m_isStarted && PreviewImage && PreviewImage.texture)
            {
                var fitter = PreviewImage.GetComponentInChildren<AspectRatioFitter>();

                if (fitter)
                {
                    fitter.aspectRatio = videoRatio;
                }
            }

            if (!hasRotated && RotateGameObject)
            {
                // TEST CASE
                //RotateGameObject.transform.Rotate(new Vector3(0, 0, -90f));

                switch (Application.platform)
                {
                case RuntimePlatform.Android:
                    var scale = RotateGameObject.transform.localScale;
                    RotateGameObject.transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
                    RotateGameObject.transform.Rotate(new Vector3(0, 0, 90f));
                    break;
                case RuntimePlatform.IPhonePlayer:
                    scale = RotateGameObject.transform.localScale;
                    RotateGameObject.transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
                    RotateGameObject.transform.Rotate(new Vector3(0, 0, 90f));
                    break;
                }

                hasRotated = true;
            }

            if (AdjustAspectScale)
            {
                var height = 2.0f;
                var width = height * videoRatio;

                if (Application.platform == RuntimePlatform.Android ||
                    Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    width = height / videoRatio;
                }

                RotateGameObject.transform.localScale = new Vector3(height, width, 0.1f);
            }*/
        }

        public virtual void StartCamera()
        {
            if (!m_isStarted)
            {
                m_isStarted = true;

                CameraManager.Instance._camera.SubscribeToPreview(this.gameObject, (tex) =>
                    {
                        Texture = tex;

                        SetTexture(tex);
                    /*
					if (PreviewRenderer)
					{
						PreviewRenderer.material.mainTexture = tex;
					}*/
                    });

                CameraManager.Instance._camera.StartCameraPreview();
            }
        }

        public virtual void StopCamera()
        {
            if (m_isStarted)
            {
                m_isStarted = false;

                CameraManager.Instance._camera.StopCameraPreview();
            }
        }

        public virtual void TakePicture(int width, int height, Action<Texture2D> onComplete)
        {
            CameraManager.Instance.TakePicture(width, height, onComplete);
        }

        public virtual Color32[] GetPixels32()
        {
            return CameraManager.Instance._camera.GetPixels32();
        }
    }

}