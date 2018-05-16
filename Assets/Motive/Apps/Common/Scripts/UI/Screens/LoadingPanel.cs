// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.Core.Media;
using Motive.UI.Framework;
using Motive.Unity.Apps;
using Motive.Unity.Globalization;
using Motive.Unity.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Main Motive splash screen. Handles various startup errors.
    /// </summary>
    public class LoadingPanel : Panel
    {
        public GameObject ResolvingPane;
        public GameObject DownloadPane;
        public GameObject ReadyPane;
        public GameObject RetryPane;
        public GameObject StartDownloadPane;
        public GameObject WaitingLocationServicesPane;
        public GameObject LocationServicesRequiredPane;

        public Text RetryText;
        public Text StartDownloadDescription;
        public Text StartDownloadButtonText;

        public Scrollbar DownloadProgress;
        public Text DownloadText;

        public bool RequireLocationServices = true;
        bool m_checkLocationServices;
        bool m_locationServiceCheckComplete;

        //bool m_mediaReady;
        WebServicesDownloadState m_currState;
        //Logger m_logger;

        MediaDownloadManager m_downloadManager;

        public event EventHandler OnDownloadError;

        // Use this for initialization
        protected override void Awake()
        {
            base.Awake();

            //m_logger = new Logger(this);
            //RetryPane.SetActive(false);
            DownloadPane.SetActive(false);
            DownloadProgress.size = 0;
        }

        public override void DidPush()
        {
            m_checkLocationServices = false;
            m_locationServiceCheckComplete = false;
            m_downloadManager = WebServices.Instance.MediaDownloadManager;

            SetState(WebServices.Instance.DownloadState);

            base.DidPush();
        }

        public void MediaReady()
        {
            m_checkLocationServices = true;
        }

        public void StartDownload()
        {
            StartDownloadPane.gameObject.SetActive(false);

            WebServices.Instance.StartDownload();
        }

        void SetState(WebServicesDownloadState state)
        {
            m_currState = state;

            DownloadPane.gameObject.SetActive(false);

            ObjectHelper.SetObjectActive(RetryPane, false);
            ObjectHelper.SetObjectActive(StartDownloadPane, false);
            ObjectHelper.SetObjectActive(ResolvingPane, false);
            ObjectHelper.SetObjectActive(ReadyPane, false);

            ObjectHelper.SetObjectActive(LocationServicesRequiredPane, false);
            ObjectHelper.SetObjectActive(WaitingLocationServicesPane, false);

            switch (state)
            {
                case WebServicesDownloadState.Resolving:
                    ObjectHelper.SetObjectActive(ResolvingPane, true);
                    break;
                case WebServicesDownloadState.Error:
                    SetWebDownloadErrorState(WebServices.Instance.DownloadErrorCode);
                    break;
                case WebServicesDownloadState.Downloading:
                    ObjectHelper.SetObjectActive(DownloadPane, true);
                    break;
                //case WebServicesDownloadState.Ready:
                //    ObjectHelper.SetObjectActive(ReadyPane, true);
                //    break;
                case WebServicesDownloadState.WaitWifi:
                    var mb = m_downloadManager.TotalBytes / 1000000;
                    StartDownloadDescription.text = string.Format("{0}MB to download. Click below to start download.", mb);
                    StartDownloadButtonText.text = string.Format("DOWNLOAD {0}MB", mb);

                    ObjectHelper.SetObjectActive(StartDownloadPane, true);
                    break;
            }
        }

        private void SetWebDownloadErrorState(WebServicesDownloadErrorCode downloadErrorCode)
        {
            if (RetryText)
            {
                switch (downloadErrorCode)
                {
                    case WebServicesDownloadErrorCode.NoNetworkConnection:
                        RetryText.text = Localize.GetLocalizedString("Loading.NoNetworkError", "An internet connection is required to launch this app. Please connect to the internet and try again.");
                        break;
                    case WebServicesDownloadErrorCode.ParseError:
                        RetryText.text = Localize.GetLocalizedString("Loading.ParseError", "There was a problem processing data from the Motive server.");
                        break;
                    case WebServicesDownloadErrorCode.ServiceCallFailure:
                        RetryText.text = Localize.GetLocalizedString("Loading.ServiceCallError", "There was a problem connecting to the Motive server. Please try again.");
                        break;
                    case WebServicesDownloadErrorCode.AuthenticationFailure:
                        RetryText.text = Localize.GetLocalizedString("Loading.AuthenticationFailure", "Failed to authenticate with the Motive server. Please try again.");
                        break;
                    case WebServicesDownloadErrorCode.APIQuotaExceeded:
                        RetryText.text = Localize.GetLocalizedString("Loading.APIQuotaExceeded", "Your API quota has been exceeded. Please try again.");
                        break;
                }
            }

            ObjectHelper.SetObjectActive(RetryPane, true);
        }

        void CheckLocationServices()
        {
            if (RequireLocationServices)
            {
                if (Application.isMobilePlatform && !Input.location.isEnabledByUser)
                {
                    ObjectHelper.SetObjectActive(WaitingLocationServicesPane, false);
                    ObjectHelper.SetObjectActive(LocationServicesRequiredPane, true);
                }
                else if (ForegroundPositionService.Instance.HasLocationData)
                {
                    m_checkLocationServices = false;
                    m_locationServiceCheckComplete = true;

                    Back();
                }
                else
                {
                    ObjectHelper.SetObjectActive(LocationServicesRequiredPane, false);
                    ObjectHelper.SetObjectActive(WaitingLocationServicesPane, true);
                }
            }
            else
            {
                m_checkLocationServices = false;
                m_locationServiceCheckComplete = true;

                Back();
            }
        }

        void Update()
        {
            if (m_currState != WebServices.Instance.DownloadState)
            {
                SetState(WebServices.Instance.DownloadState);
            }

            if (m_currState == WebServicesDownloadState.Downloading)
            {
                SetDownloadStatus(m_downloadManager.TotalBytesRead, m_downloadManager.TotalBytes);
            }

            if (m_currState == WebServicesDownloadState.Ready &&
                m_checkLocationServices)
            {
                CheckLocationServices();
            }
            else if (m_locationServiceCheckComplete)
            {
                ObjectHelper.SetObjectActive(ReadyPane, true);
            }
        }

        public void SetDownloadStatus(long bytesRead, long totalBytes)
        {
            if (DownloadText)
            {
                if (totalBytes == 0)
                {
                    DownloadText.text = "";
                }
                else if (totalBytes >= 100000000)
                {
                    DownloadText.text = string.Format("{0:0}MB / {1:0}MB", (double)bytesRead / 1000000, (double)totalBytes / 1000000);
                }
                else if (totalBytes >= 1000000)
                {
                    DownloadText.text = string.Format("{0:0.0}MB / {1:0.0}MB", (double)bytesRead / 1000000, (double)totalBytes / 1000000);
                }
                else
                {
                    DownloadText.text = string.Format("{0:0.0}k / {1:0.0}k", (double)bytesRead / 1000, (double)totalBytes / 1000);
                }
            }

            if (totalBytes == 0)
            {
                DownloadProgress.size = 1;
            }
            else
            {
                DownloadProgress.size = (float)((double)bytesRead / totalBytes);
            }
        }

        public void Retry()
        {
            RetryPane.gameObject.SetActive(false);

            if (DynamicConfig.UseDynamicConfig)
            {
                Back();

                DynamicConfig.Instance.Retry();
            }
            else
            {
                AppManager.Instance.Reload();
            }
        }

        internal void SetStatus(string p)
        {
            DownloadText.gameObject.SetActive(true);
            DownloadText.text = p;
        }

        public void DownloadError(string p)
        {
            SetStatus(p);
            if (OnDownloadError != null) OnDownloadError(this, EventArgs.Empty);
            OnDownloadError = null;
        }
    }
}
