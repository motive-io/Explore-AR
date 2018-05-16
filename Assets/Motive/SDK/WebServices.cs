// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core;
using Motive.Core.Json;
using Motive.Core.Media;
using Motive.Core.Models;
using Motive.Core.Scripting;
using Motive.Core.Storage;
using Motive.Core.Utilities;
using Motive.Core.WebServices;
using Motive.Unity.Storage;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive
{
    /// <summary>
    /// Whether to use dev catalogs and configs.
    /// </summary>
    public enum DevCatalogMode
    {
        /// <summary>
        /// Use dev catalogs and configs.
        /// </summary>
        Yes,
        /// <summary>
        /// Use published catalogs.
        /// </summary>
        No,
        /// <summary>
        /// Use dev files in debug mode and published files in release mode.
        /// </summary>
        OnlyInDebugMode
    }

    /// <summary>
    /// Defines how data from the Motive server is stored.
    /// </summary>
    public enum WebServiceStorageMode
    {
        /// <summary>
        /// Downloaded and cached.
        /// </summary>
        CachedFromWeb,
        /// <summary>
        /// No caching, streamed from the web.
        /// </summary>
        WebOnly,
        /// <summary>
        /// Data is stored in the app offline.
        /// </summary>
        Offline
    }

    public enum WebServicesDownloadState
    {
        None,
        Error,
        Resolving,
        WaitWifi,
        Downloading,
        Ready
    }

    public enum WebServicesDownloadErrorCode
    {
        AuthenticationFailure,
        ServiceCallFailure,
        ParseError,
        NoNetworkConnection,
        APIQuotaExceeded
    }
    
    public class WebServicesStateEventArgs : EventArgs
    {
        public WebServicesDownloadState DownloadState { get; private set; }

        public WebServicesStateEventArgs(WebServicesDownloadState state)
        {
            DownloadState = state;
        }
    }

    /// <summary>
    /// Manages the connection to Motive server. The values on this object are
    /// configured using an AppConfiguration object.
    /// </summary>
    public class WebServices : SingletonComponent<WebServices>
    {
        public EventHandler<WebServicesStateEventArgs> DownloadStateChanged;
        public EventHandler<WebServicesStateEventArgs> DownloadError;

        public string MotiveUrl = "https://api.motive.io";

        public string AppId = "YOUR MOTIVE APP ID";
        public string ApiKey = "YOUR MOTIVE API KEY";
        public string SpaceName { get; private set; }

        public string UserDomain;

        public bool UseDebugReachability = false;
        public NetworkReachability DebugReachability;

        public string ConfigName;

        private bool m_isEditor;

        // TODO: not enabled yet: public WebServiceStorageMode StorageMode = WebServiceStorageMode.CachedFromWeb;

        public NetworkReachability InternetReachability
        {
            get
            {
                    if (m_isEditor && UseDebugReachability)
                    {
                        return DebugReachability;
                    }
                return Application.internetReachability;
            }
        }

        public DevCatalogMode UseDevCatalogs = DevCatalogMode.OnlyInDebugMode;

        public bool RequireNetworkConnection = true;
        public MediaDownloadManager MediaDownloadManager { get; private set; }
        public WebServicesDownloadErrorCode DownloadErrorCode { get; private set; }
        public WebServicesDownloadState DownloadState { get; private set; }

        private UserManager m_UserManager { get; set; }

        public UserManager UserManager
        {
            get
            {
                if (UserDomain != null)
                    return m_UserManager ?? (m_UserManager = new UserManager(MotiveUrl, SpaceName + "." + UserDomain));
                else
                {
                    throw new Exception("Cannot instantiate UseerManager. UserDomain not set.");
                }
            }
            private set { m_UserManager = value; }
        }

        public bool ShouldUseDevCatalogs
        {
            get
            {
                return UseDevCatalogs == DevCatalogMode.Yes || 
                                         (BuildSettings.IsDebug && UseDevCatalogs == DevCatalogMode.OnlyInDebugMode);
            }
        }

        /// <summary>
        /// Threshold below which media will be downloaded over the cell
        /// network.
        /// </summary>
        public int CellDownloadThreshold = 25;

        private CatalogLoader m_catalogLoader;

        private CatalogLoader CatalogLoader
        {
            get { return m_catalogLoader ?? (m_catalogLoader = new CatalogLoader(MotiveUrl, SpaceName)); }
            set { m_catalogLoader = value; }
        }
        private Logger m_logger;

        //private int m_totalFiles;
        //private long m_totalSize;

        Action m_completeSuccessHandler;
        Action<string> m_completeFailHandler;

        List<Action<Action>> m_catalogLoadActions;
        List<Action> m_preloadActions;

        protected override void Awake()
        {
            base.Awake();

            m_isEditor = Application.isEditor;

            m_logger = new Logger(this);
            m_catalogLoadActions = new List<Action<Action>>();
            m_preloadActions = new List<Action>();
        }
        
        private void SetDownloadError(ServiceCallLoadStatus status)
        {
            switch (status)
            {
                case ServiceCallLoadStatus.ServiceCallFail:
                    SetDownloadError(WebServicesDownloadErrorCode.ServiceCallFailure);
                    break;
                case ServiceCallLoadStatus.ParseFail:
                    SetDownloadError(WebServicesDownloadErrorCode.ParseError);
                    break;
                case ServiceCallLoadStatus.APIQuotaExceeded:
                    SetDownloadError(WebServicesDownloadErrorCode.APIQuotaExceeded);
                    break;
                case ServiceCallLoadStatus.AuthenticationFailure:
                    SetDownloadError(WebServicesDownloadErrorCode.AuthenticationFailure);
                    break;
                default:
                    throw new InvalidOperationException("Unrecognized error: " + status);
            }
        }

        void SetDownloadError(WebServicesDownloadErrorCode errorCode)
        {
            DownloadErrorCode = errorCode;

            SetDownloadState(WebServicesDownloadState.Error);
        }

        void SetDownloadState(WebServicesDownloadState state)
        {
            DownloadState = state;
            var args = new WebServicesStateEventArgs(state);

            ThreadHelper.Instance.CallOnMainThread(() =>
                {
                    if (DownloadStateChanged != null)
                    {
                        DownloadStateChanged(this, args);
                    }

                    if (state == WebServicesDownloadState.Error)
                    {
                        DownloadError(this, args);
                    }
                });
        }

        public void BeforeLoad(Action callback)
        {
            m_preloadActions.Add(callback);
        }

        public void SetSpaceName(string _spaceName)
        {
            SpaceName = _spaceName;
        }

        public void LoadCatalog<T>(string spaceName, string catalogName, Action<Catalog<T>> onLoad, bool? useDevCatalogs = null)
        {
            if (string.IsNullOrEmpty(catalogName))
            {
                return;
            }

            var fileName = StorageManager.GetCatalogFileName(spaceName, catalogName + ".json");

            // If the caller has specified a preference for using dev catalogs, use that,
            // otherwise defer to the setting here | the debug setting
            bool useDev = useDevCatalogs ??
                (ShouldUseDevCatalogs || SettingsHelper.IsDebugSet("Debug_UseDevCatalogs"));

            if (InternetReachability == NetworkReachability.NotReachable)
            {
                if (RequireNetworkConnection)
                {
                    SetDownloadError(WebServicesDownloadErrorCode.NoNetworkConnection);
                }
                else
                {
                    var catalog = CatalogLoader.LoadCatalogFromCache<T>(fileName);

                    if (catalog == null)
                    {
                        if (m_completeFailHandler != null)
                        {
                            m_completeFailHandler("Please connect to the network and re-try");
                        }
                    }
                    else
                    {
                        MediaDownloadManager.AddMediaItemProvider(catalog);

                        onLoad(catalog);
                    }
                }
            }
            else
            {
                CatalogLoader.LoadCatalog<T>(fileName, spaceName, catalogName, useDev,
                    (status, catalog) =>
                    {
                        if (status == ServiceCallLoadStatus.Success)
                        {
                            m_logger.Debug("Loaded catalog {0} with {1} item(s)",
                                catalogName, catalog.Items == null ? 0 : catalog.Items.Length);

                            // This callback happens outside of the Unity thread,
                            // use the Thread Helper to move into the Unity context
                            ThreadHelper.Instance.CallOnMainThread(() =>
                                {
                                    MediaDownloadManager.AddMediaItemProvider(catalog);

                                    onLoad(catalog);

                                    // Since we're in the Unity thread here we don't need
                                    // to protect this in a critical section
                                });
                        }
                        else
                        {
                            m_logger.Error("Error loading catalog {0}", catalogName);

                            SetDownloadError(status);

                            if (m_completeFailHandler != null)
                            {
                                ThreadHelper.Instance.CallOnMainThread(() =>
                                {
                                    m_completeFailHandler("Error loading catalog " + catalogName);
                                });
                            }
                        }
                    });
            }
        }

        public void LoadCatalog<T>(string catalogName, Action<Catalog<T>> onLoad)
        {
            LoadCatalog<T>(SpaceName, catalogName, onLoad, ShouldUseDevCatalogs);
        }

        // Use this for initialization
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <exception cref="Exception">Motive URL not set in WebServices</exception>
        public void Initialize()
        {
            SpaceName = !string.IsNullOrEmpty(AppId) ? AppId.Substring(0, AppId.LastIndexOf('.')) : null;

#if !UNITY_WP8
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                new System.Net.Security.RemoteCertificateValidationCallback(
                    (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                        System.Security.Cryptography.X509Certificates.X509Chain chain,
                        System.Net.Security.SslPolicyErrors sslPolicyErrors) =>
                    {
                        return true;
                    });
#endif
            var mediaFolder = StorageManager.EnsureDownloadsFolder("media");

            MediaDownloadManager = new MediaDownloadManager(mediaFolder);

            // catch MotiveUrl not set error
            if (string.IsNullOrEmpty(MotiveUrl))
            {
                m_logger.Error("Motive URL not set in WebServices");
                throw new Exception("Motive URL not set in WebServices");
            }

            MotiveAuthenticator.Instance.Initialize(
                MotiveUrl.Replace(" ", string.Empty),
                AppId.Replace(" ", string.Empty),
                ApiKey.Replace(" ", string.Empty),
                new FileStorageAgent(StorageManager.GetFilePath("authenticator", "authenticationState.json")));

            //m_catalogLoader = new CatalogLoader(MotiveUrl, SpaceName, UseDevCatalogs);
        }

        public void AuthenticateQRToken(string token, Action<bool, QRTokenSpaceModel> OnResponse)
        {
            m_logger.Debug("deeplink Authenticating QR Token with server...");
            var url = string.Format("{0}/api/shareTokens/{1}/", MotiveUrl, token);

            var serviceCall = new ServiceCall(new Uri(url), HttpMethod.Get);

            MotiveAuthenticator.Instance.MakeClientCall(serviceCall, (status) =>
            {
                m_logger.Debug("deeplink MakeClientCall");
                var serverJson = serviceCall.ResponseText;
                Debug.Log("deeplink just before serverjson log");
                Debug.Log("deeplink serverJSON:\n " + serverJson);
                var qrTokModel = JsonHelper.GetReader(serverJson).Deserialize<QRTokenSpaceModel>();
                
                if (qrTokModel != null && qrTokModel.Space != null && qrTokModel.ProjectConfig != null 
                    && !string.IsNullOrEmpty(qrTokModel.ProjectConfig.Name))
                {
                    m_logger.Debug("deeplink call load space...");

                    if (OnResponse != null) OnResponse.Invoke(status == AuthenticatorCallStatus.Success, qrTokModel);
                }
                else
                {
                    if (OnResponse != null) OnResponse.Invoke(false, null);

                    m_logger.Debug("deeplink response not populated");
                }
            });
        }

        public void AddAssetDirectory<T>(AssetDirectory<T> directory) where T : ScriptObject
        {
            BeforeLoad(directory.Clear);

            m_catalogLoadActions.Add((onComplete) =>
            {
                var cats = ScriptObjectDirectory.Instance.GetAllCatalogs<T>();

                if (cats != null)
                {
                    foreach (var cat in cats)
                    {
                        directory.AddCatalog(cat);
                    }
                }

                onComplete();
            });
        }

        public void AddCatalogLoad<T>(string catalogName, AssetDirectory<T> directory, bool? useDevCatalog = null)
            where T : ScriptObject
        {
            if (string.IsNullOrEmpty(catalogName))
            {
                return;
            }

            BeforeLoad(directory.Clear);

            m_catalogLoadActions.Add((onComplete) =>
            {
                LoadCatalog<T>(SpaceName, catalogName, (cat) =>
                {
                    directory.AddCatalog(cat);

                    onComplete();
                }, useDevCatalog);
            });
        }

        public void AddCatalogLoad<T>(string catalogName, Action<Catalog<T>> onLoad, bool? useDevCatalog = null)
        {
            if (string.IsNullOrEmpty(catalogName))
            {
                return;
            }

            m_catalogLoadActions.Add((onComplete) =>
            {
                LoadCatalog<T>(SpaceName, catalogName, (cat) =>
                {
                    onLoad(cat);

                    onComplete();
                }, useDevCatalog);
            });
        }

        public void AddCatalogLoad<T>(string spaceName, string catalogName, Action<Catalog<T>> onLoad, bool? useDevCatalog = null)
        {
            if (string.IsNullOrEmpty(catalogName))
            {
                return;
            }

            m_catalogLoadActions.Add((onComplete) =>
            {
                LoadCatalog<T>(spaceName, catalogName, (cat) =>
                {
                    onLoad(cat);

                    onComplete();
                }, useDevCatalog);
            });
        }

        public void ReloadFromServer(Action onSuccess, Action<string> onFail)
        {
            MediaDownloadManager.Reset();

            m_completeSuccessHandler = onSuccess;
            m_completeFailHandler = onFail;

            foreach (var call in m_preloadActions.ToArray())
            {
                call();
            }

            SetDownloadState(WebServicesDownloadState.Resolving);

            Action configLoadComplete = () =>
            {
                ThreadHelper.Instance.CallOnMainThread(() =>
                {
                    foreach (var obj in ScriptObjectDirectory.Instance.AllObjects)
                    {
                        if (obj is IMediaItemProvider)
                        {
                            MediaDownloadManager.AddMediaItemProvider((IMediaItemProvider)obj);
                        }
                    }

                    if (m_catalogLoadActions.Count > 0)
                    {
                        BatchProcessor iter = new BatchProcessor(m_catalogLoadActions.Count, DownloadMedia);

                        foreach (var call in m_catalogLoadActions)
                        {
                            call(() => { iter++; });
                        }
                    }
                    else
                    {
                        DownloadMedia();
                    }
                });
            };

            if (!string.IsNullOrEmpty(ConfigName))
            {
                var cfgManager = StorageManager.GetAppStorageManager().GetManager("downloads", SpaceName, "config");

                ProjectConfigService.Instance.Load(cfgManager, MotiveUrl, SpaceName, ConfigName, ShouldUseDevCatalogs, (status) =>
                {
                    if (status == ServiceCallLoadStatus.Success)
                    {
                        configLoadComplete();
                    }
                    else
                    {
                        SetDownloadError(status);
                    }
                });
            }
            else
            {
                configLoadComplete();
            }
        }

        public void StartDownload()
        {
            if (MediaDownloadManager.OutstandingFileSize > 0)
            {
                SetDownloadState(WebServicesDownloadState.Downloading);

                //m_totalFiles = MediaDownloadManager.OutstandingFileCount;
                //m_totalSize = MediaDownloadManager.OutstandingFileSize;

                MediaDownloadManager.DownloadAll((dlSuccess) =>
                {
                    ThreadHelper.Instance.CallOnMainThread(() =>
                    {
                        if (!dlSuccess)
                        {
                            SetDownloadState(WebServicesDownloadState.Error);

                            if (m_completeFailHandler != null)
                            {
                                m_completeFailHandler("Error downloading media. Try again.");
                            }
                        }
                        else
                        {
                            SetDownloadState(WebServicesDownloadState.Ready);

                            if (m_completeSuccessHandler != null)
                            {
                                m_completeSuccessHandler();
                            }
                        }
                    });
                });
            }
            else
            {
                SetDownloadState(WebServicesDownloadState.Ready);

                if (m_completeSuccessHandler != null)
                {
                    m_completeSuccessHandler();
                }
            }
        }

        void DownloadMedia()
        {
            SetDownloadState(WebServicesDownloadState.Resolving);

            // Catalogs ready! Download media if required
            MediaDownloadManager.Resolve((resSuccess) =>
            {
                ThreadHelper.Instance.CallOnMainThread(() =>
                {
                    if (resSuccess)
                    {
                        if (InternetReachability == NetworkReachability.ReachableViaCarrierDataNetwork &&
                            MediaDownloadManager.OutstandingFileSize > CellDownloadThreshold * 1000000)
                        {
                            SetDownloadState(WebServicesDownloadState.WaitWifi);
                        }
                        else
                        {
                            StartDownload();
                        }
                    }
                    else
                    {
                        SetDownloadState(WebServicesDownloadState.Error);

                        m_logger.Error("Failed to resolve media.");
                    }
                });
            });
        }

        public void SetConfig(string configName)
        {
            this.ConfigName = configName;
        }
    }
}