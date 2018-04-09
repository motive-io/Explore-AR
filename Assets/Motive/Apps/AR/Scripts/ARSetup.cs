// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using Motive.AR.LocationServices;
using Motive.Core.Scripting;
using Motive.Unity.Maps;
using Motive.AR.Media;
using Motive.Core;

using Logger = Motive.Core.Diagnostics.Logger;
using Motive.Unity.Scripting;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;

#if MOTIVE_MAPBOX
using Mapbox.Unity;
#endif

namespace Motive.Unity.Apps
{
    /// <summary>
    /// Base class for setup for Location-AR apps.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ARSetup<T> : MonoBehaviour where T : ARSetup<T>
    {
        public AppConfig AppConfig;
        public bool AllowMultipleScriptRunners;
        public bool AutoLaunchScriptRunners;
        public bool RequireLogin;
        public bool PreLoadAssetBundles;

        static T sInstance = null;
        static bool m_isInitialized;

        private Logger m_logger;

        public static T Instance
        {
            get { return sInstance; }
        }

        protected virtual void Awake()
        {
            m_logger = new Logger(this);

            if (sInstance != null)
            {
                m_logger.Error("SingletonComponent.Awake: error " + name + " already initialized");
            }

            sInstance = (T)this;
        }

        // Use this for initialization
        protected virtual void Start()
        {
            if (!m_isInitialized)
            {
                Platform.Instance.Initialize();

                if (AppConfig)
                {
                    WebServices.Instance.AppId = AppConfig.AppId;
                    WebServices.Instance.ApiKey = AppConfig.ApiKey;
                    WebServices.Instance.SetConfig(AppConfig.ConfigName);
                }

                WebServices.Instance.Initialize();

                DebugPlayerLocation.Instance.Initialize();

                ScriptExtensions.Initialize(
                    Platform.Instance.LocationManager,
                    Platform.Instance.BeaconManager,
                    ForegroundPositionService.Instance.Compass);

                ARScriptExtensions.Initialize();

                ForegroundPositionService.Instance.Initialize();

                ScriptRunnerManager.Instance.AllowMultiple = AllowMultipleScriptRunners;

                WebServices.Instance.AddAssetDirectory(CharacterDirectory.Instance);
                WebServices.Instance.AddAssetDirectory(CollectibleDirectory.Instance);
                WebServices.Instance.AddAssetDirectory(ScriptRunnerDirectory.Instance);
                WebServices.Instance.AddAssetDirectory(RecipeDirectory.Instance);

                if (PreLoadAssetBundles)
                {
                    WebServices.Instance.AddAssetDirectory(new AssetDirectory<Motive.Unity.Models.AssetBundle>());
                }

                AppManager.Instance.OnLoadComplete(() =>
                    {
                        var objects = ScriptObjectDirectory.Instance.GetAllObjects<Script>();
                        ScriptManager.Instance.SetScripts(objects);

                        if (PreLoadAssetBundles)
                        {
                            UnityAssetLoader.PreloadBundles(ScriptObjectDirectory.Instance.GetAllObjects<Motive.Unity.Models.AssetBundle>());
                        }
                    });

                if (AutoLaunchScriptRunners)
                {
                    ScriptManager.Instance.ScriptsUpdated += (caller, args) =>
                        {
                            ScriptRunnerManager.Instance.LaunchRunningScripts(ScriptRunnerDirectory.Instance);
                        };
                }

                ConfigureLocationServices();

                if (DynamicConfig.Instance != null && DynamicConfig.Instance.UseDynamicConfigLogin)
                {
                    DynamicConfig.Instance.SpaceSelected += (sender, args) =>
                    {
                        if (!AppManager.Instance.IsInitialized)
                        {
                            AppManager.Instance.Start();
                        }
                        else
                        {
                            AppManager.Instance.Reset();
                        }
                    };

                    DynamicConfig.Instance.Initialize();
                }
                else
                {
                    AppManager.Instance.Start();
                }

                ForegroundPositionService.Instance.LocationTracker.GetReading((reading) =>
                {
                    OnFirstLocationReading(reading);
                });

                WorldValuablesManager.Instance.Initialize();

                LocativeAudioDriver.Instance.Start();

                Platform.Instance.StartSensors();

                TodoManager.Instance.Initialize();

                m_isInitialized = true;


                // debug: look for push panel here
                //if (QueuePanelManager.Instance != null && QueuePanelManager.Instance.IsPanelQueued)
                //{
                //    QueuePanelManager.Instance.PopQueuedPanel();
                //}
            }
        }

        protected virtual void OnFirstLocationReading(LocationReading reading)
        {
        }

        protected virtual void ConfigureLocationServices()
        {
#if MOTIVE_MAPBOX
            if (!string.IsNullOrEmpty(MapboxAccess.Instance.Configuration.AccessToken) &&
                MapboxLocationCacheDriver.Instance != null)
            {
                MapboxLocationCacheDriver.Instance.Initialize(MapboxAccess.Instance.Configuration.AccessToken);

                var source = GameObject.FindObjectOfType<MapBoxSource>();

                if (source)
                {
                    source.AccessToken = MapboxAccess.Instance.Configuration.AccessToken;
                }
            }
#endif
        }
    }
}