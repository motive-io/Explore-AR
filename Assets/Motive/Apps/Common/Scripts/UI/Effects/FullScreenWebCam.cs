// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;

#if MOTIVE_USES_CAMERA
namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays a webcam texture fullscreen.
    /// </summary>
    public class FullScreenWebCam : MonoBehaviour
    {

        WebCamTexture m_cameraTexture;
        bool m_startPlaying;

        void Awake()
        {
            var height = 2.0f;
            var width = height * Screen.width / Screen.height;
            transform.parent.localScale = new Vector3(width, height, 0.1f);
        }

        // Use this for initialization
        void Start()
        {
            Debug.Log("Initialize - rot:" + transform.localRotation.eulerAngles);
        }

        WebCamTexture CreateWebcamTexture()
        {
            Debug.Log("CreateTex - rot:" + transform.localRotation.eulerAngles);

            //        var BackgroundTexture = gameObject.AddComponent<GUITexture>();
            //        BackgroundTexture.pixelInset = new Rect(0, 0, Screen.width, Screen.height);
            //set up camera
            WebCamDevice[] devices = WebCamTexture.devices;
            string backCamName = null;

            for (int i = 0; i < devices.Length; i++)
            {
                Debug.Log("Device:" + devices[i].name + "IS FRONT FACING:" + devices[i].isFrontFacing);

#if UNITY_EDITOR
                backCamName = devices[i].name;
                break;
#else
            if (!devices[i].isFrontFacing)
            {
                backCamName = devices[i].name;
            }
#endif
            }

            if (backCamName != null)
            {
                var camTex = new WebCamTexture(backCamName);
                camTex.Play();

#if UNITY_IOS
			var scale = transform.localScale;
			transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
			transform.Rotate(Vector3.forward, camTex.videoRotationAngle);
#endif

                if (Application.platform == RuntimePlatform.Android)
                {
                    transform.Rotate(Vector3.forward, -90);
                }
                return camTex;
            }

            return null;
        }

        void StartPlaying()
        {
            if (!m_cameraTexture)
            {
                m_cameraTexture = CreateWebcamTexture();
                //m_cameraTexture = WebCamHelper.CreateWebcamTexture(WebCamFacing.Back, this.gameObject);
                //m_cameraTexture.Play();
            }
            else
            {
                m_cameraTexture.Play();
            }

            if (m_cameraTexture)
            {
                GetComponent<Renderer>().material.mainTexture = m_cameraTexture;
                m_startPlaying = false;
            }
        }

        public void Play()
        {
            m_startPlaying = true;
        }

        public void Stop()
        {
            if (m_cameraTexture)
            {
                m_cameraTexture.Stop();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (m_startPlaying)
            {
                StartPlaying();
            }

            if (m_cameraTexture)
            {
                var height = 2.0f;
                var width = height * m_cameraTexture.width / m_cameraTexture.height;

                if (Application.platform == RuntimePlatform.Android ||
                    Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    //width = height * m_cameraTexture.height / m_cameraTexture.width;
                }

                transform.parent.localScale = new Vector3(height, width, 0.1f);
            }
        }
    }

}
#endif