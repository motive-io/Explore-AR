// Copyright (c) 2018 RocketChicken Interactive Inc.
#if UNITY_IOS && NATIVE_PLUGINS
#define IOS_NATIVE
#endif
#if UNITY_ANDROID && NATIVE_PLUGINS
#define ANDROID_NATIVE
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Motive.Core.Media;
using Motive.Core.WebServices;
using Motive.Unity.Media;
using Motive.Core.Sensors;
using Motive.Core.Notifications;
using Motive.Unity.Sensors;
using Motive.AR.Kinetics;
using Motive.AR.LocationServices;
using Motive.AR.Beacons;
using Motive.Core.Scripting;
using System;
using Motive.Unity.Utilities;
using UnityEngine.Events;
using Motive.AR.Media;
using Motive.Core.Diagnostics;
using Logger = Motive.Core.Diagnostics.Logger;
using Motive.Unity;
using Motive.Unity.Playables;
using Motive.Unity.Beacons;
#if IOS_NATIVE
using Motive.IOS.Beacons;
using Motive.IOS.Sensors;
using Motive.IOS.WebServices;
using Motive.IOS.Media;
using Motive.IOS.LocationServices;
using Motive.IOS.Notifications;
#endif
#if ANDROID_NATIVE
using Motive.Android.Media;
using Motive.Android.LocationServices;
using Motive.Android.Java;
using Motive.Android.Application;
using Motive.Android.Beacons;
using Motive.Android.Notfications;
using Motive.Unity.Storage;
#endif

namespace Motive
{
    /// <summary>
    /// Defines the screen sleep behavior.
    /// </summary>
    public enum ScreenSleepBehavior
    {
        /// <summary>
        /// Default behavior set by the app.
        /// </summary>
        SystemDefault,
        /// <summary>
        /// Screen is always on while a live session is running.
        /// </summary>
        OnDuringLiveSessions,
        /// <summary>
        /// Screen is always on.
        /// </summary>
        AlwaysOn
    }

    /// <summary>
    /// Defines different compass types supported by Motive.
    /// </summary>
	public enum CompassType
	{
        /// <summary>
        /// Compass readings directly from the device compass.
        /// </summary>
		System,
        /// <summary>
        /// Applies dampening to the compass readings to reduce jitter.
        /// </summary>
		NoiseReduced,
        /// <summary>
        /// Uses location updates to determine the player's heading.
        /// </summary>
		LocationTracker,
        /// <summary>
        /// Combination of NoiseReduced when the app is in the foreground and
        /// LocationTracker when it moves to the background.
        /// </summary>
		Hybrid
	}

    /// <summary>
    /// Main integration point for platform-specific features.
    /// </summary>
    public class Platform : SingletonComponent<Platform>
    {
		[Header("Events")]
        public UnityEvent OnEnterBackground;
        public UnityEvent OnExitBackground;
        public UnityEvent OnPauseAudio;
        public UnityEvent OnResumeAudio;

		[Header("Storage Configuration")]
        public bool UseEncryption;
        public string EncryptionKey;
        public string EncryptionIV;

        /// <summary>
        /// Folder for cache items.
        /// </summary>
        public string CachePath { get; private set; }

        /// <summary>
        /// Folder for user data.
        /// </summary>
		public string UserDataPath { get; private set; }

        /// <summary>
        /// Folder for app data.
        /// </summary>
		public string AppDataPath { get; private set; }

        /// <summary>
        /// Streaming assets folder. Cached on startup so background threads can
        /// read it.
        /// </summary>
        public string StreamingAssetsPath { get; private set; }

        /// <summary>
        /// If true, uses a native implementation of the file downloader (iOS only).
        /// This can improve download performance by up to 50%.
        /// </summary>
        [Header("Native Hooks")]        
        public bool UseNativeDownloader;
        /// <summary>
        /// Enable background location if native plugins are available.
        /// </summary>
        public bool EnableBackgroundLocation;
        /// <summary>
        /// Enable background audio if native plugins are available.
        /// </summary>
		public bool EnableBackgroundAudio;
        /// <summary>
        /// Enable system notifications if native plugins are available.
        /// </summary>
		public bool EnableNotifications;
        /// <summary>
        /// Enable system background notifications if native plugins are available.
        /// </summary>
		public bool EnableBackgroundNotifications;
        /// <summary>
        /// Enable beacons if native plugins are available.
        /// </summary>
        public bool EnableBeacons;

		#if IOS_NATIVE
		[Header("IOS Audio Settings")]
		public IOSAudioSessionCategory DefaultAudioCategory;
		public bool MixWithOthers;

		IOSAudioSessionCategory m_currentCategory;
		IOSAudioSessionCategoryOptions m_categoryOptions;
		#endif

        [Header("Screen Sleep Behaviour")]
        public ScreenSleepBehavior ScreenSleepBehavior = ScreenSleepBehavior.SystemDefault;

		[Header("Compass Type")]
		public CompassType CompassType = CompassType.Hybrid;

		[Header("Location Manager")]
		public bool UseDeadReckoning = true;
        public bool SmoothLocationUpdates = true;

        /// <summary>
        /// Returns true if there is currently a live session requested.
        /// </summary>
        public bool IsLiveSession
        {
            get
            {
                return m_liveSessionListeners != null &&
                    m_liveSessionListeners.Count > 0;
            }
        }

        /// <summary>
        /// Returns true if there is a live session running, but it has been suspended.
        /// </summary>
		public bool IsLiveSessionSuspended { get; private set; }

        /// <summary>
        /// Returns true if there is currently a live session running (IsLiveSession && !IsLiveSessionSuspended)
        /// </summary>
        public bool IsLiveSessionRunning 
		{
			get
			{
				return IsLiveSession && !IsLiveSessionSuspended;
			}
		}

        /// <summary>
        /// Provides access to the device's accelerometer.
        /// </summary>
        public IAccelerometer Accelerometer { get; private set; }

        /// <summary>
        /// Provides "step" events.
        /// </summary>
        public Pedometer Pedometer { get; private set; }

        /// <summary>
        /// Provides access to GPS data.
        /// </summary>
        public ILocationManager LocationManager { get; private set; }

        /// <summary>
        /// A compass with reduced jitter.
        /// </summary>
        public ICompass NoiseReducedCompass { get; private set; }

        /// <summary>
        /// Provides direct readings from the device's compass.
        /// </summary>
        public ICompass SystemCompass { get; private set; }

        /// <summary>
        /// A compass that uses the user's tracked location to derive a heading.
        /// </summary>
        public ICompass LocationTrackerCompass { get; private set; }
        
		public IBeaconManager BeaconManager { get; private set; }
		public ILocalNotificationManager LocalNotificationManager { get; private set; }

        /// <summary>
        /// Audio channel that uses the default from CreateAudioChannel.
        /// </summary>
        public IAudioPlayerChannel AudioChannel { get; private set; }

        /// <summary>
        /// Audio channel that only plays in the foreground (intended for UI-bound sounds).
        /// </summary>
        public IAudioPlayerChannel ForegroundAudioChannel { get; private set; }

		public ICompass Compass;

        /// <summary>
        /// Returns true if the app is currently running in the background.
        /// </summary>
		public bool IsInBackground { get; private set; }

        private HashSet<object> m_liveSessionListeners;

		private Logger m_logger;

        private void OnLowMemory()
        {
            ImageLoader.ClearCache();

            m_logger.Log(LogLevel.Warning,  "Low Memory Warning.");
        }

        protected override void Awake()
        {
			m_logger = new Logger(this);

            Application.lowMemory += OnLowMemory;

            // Cache this so we can grab it off-thread
            StreamingAssetsPath = Application.streamingAssetsPath;
            UserDataPath = AppDataPath = Application.persistentDataPath;
            CachePath = UnityEngine.Application.temporaryCachePath;

            if (OnEnterBackground == null)
            {
                OnEnterBackground = new UnityEvent();
            }

            if (OnExitBackground == null)
            {
                OnExitBackground = new UnityEvent();
            }

            if (OnPauseAudio == null)
            {
                OnPauseAudio = new UnityEvent();
            }

            if (OnResumeAudio == null)
            {
                OnResumeAudio = new UnityEvent();
            }

            base.Awake();
        }

        public void Initialize()
		{
            m_liveSessionListeners = new HashSet<object>();

            IStepTracker stepTracker = null;

#if IOS_NATIVE
			if (Application.isMobilePlatform)
			{
				m_logger.Debug("Initializing iOS Native");

				m_categoryOptions = IOSAudioSessionCategoryOptions.None;

				if (MixWithOthers)
				{
					m_categoryOptions = IOSAudioSessionCategoryOptions.MixWithOthers;
				}

				m_currentCategory = DefaultAudioCategory;

                if (EnableBackgroundAudio)
                {
                    IOSAudioSession.Instance.SetCategory(m_currentCategory, m_categoryOptions);

                    IOSAudioSession.Instance.RemoteCommandReceived += (sender, args) =>
                        {
                            if (args.Command == IOSAudioRemoteCommand.Pause)
                            {
                                SuspendLiveSession();
                            }
                        };
                }

				if (UseNativeDownloader)
				{
					FileDownloader.SetFactory(new IOSFileDownloaderFactory());
				}

				AppDataPath = IOSFileDownloader.AppDataPath;

				Accelerometer = IOSMotionManager.Instance.GetAccelerometer();
				LocationManager = IOSLocationManager.Instance;
				SystemCompass = new IOSCompass();

                if (EnableBeacons)
                {
    				BeaconManager = IOSBeaconManager.Instance;
                }

				if (EnableNotifications)
				{					
					LocalNotificationManager = 
						new IOSLocalNotificationManager(IOSLocalNotificationTypes.Badge | IOSLocalNotificationTypes.Sound);
				}
			}
#endif
#if ANDROID_NATIVE
            if (Application.isMobilePlatform)
            {
				m_logger.Debug("Initializing Android Native");

                JavaClass activityClass = new JavaClass("com.unity3d.player.UnityPlayer");
                JavaObject mainActivity = activityClass.GetStaticFieldValue<JavaObject>("currentActivity", "android.app.Activity");

                AndroidApplicationManager.Instance.Initialize(mainActivity);

                LocationManager = new AndroidLocationManager();
            
                if (EnableBeacons)
                {
                    BeaconManager = new AndroidBeaconManager();
                }

				if (EnableNotifications)
				{
					LocalNotificationManager = new AndroidLocalNotificationManager(StorageManager.GetFilePath("platform", "androidNotificationManager.json"));
				}
            }
#endif

            if (BeaconManager == null && EnableBeacons)
            {
                BeaconManager = UnityBeaconManager.Instance;
            }

            if (Accelerometer == null)
            {
                Accelerometer = gameObject.AddComponent<UnityAccelerometer>();
            }

            if (LocationManager == null)
            {
                LocationManager = gameObject.AddComponent<UnityLocationManager>();
            }

            LocationManager.EnableBackgroundUpdates = EnableBackgroundLocation;

            if (SystemCompass == null)
            {
                SystemCompass = gameObject.AddComponent<UnityCompass>();
            }

			if (LocalNotificationManager == null)
			{
				LocalNotificationManager = new DummyLocalNotificationManager();
			}

            if (EnableNotifications && EnableBackgroundNotifications)
            {
                BackgroundNotifier.Instance.Initialize();
            }

            var nrCompass = new NoiseDampeningCompass(SystemCompass);

#if UNITY_ANDROID
            // Android compass tends to be much more jittery--apply a higher dampening
            // factor
            nrCompass.DampeningFactor = 0.25;
#endif 

            NoiseReducedCompass = nrCompass;

            if (Application.isMobilePlatform)
            {
                var accTracker = new AccelerometerStepTracker(Accelerometer);

                stepTracker = accTracker;

                Accelerometer.Start();
                accTracker.Start();
            }
            else
            {
                stepTracker = gameObject.AddComponent<DebugStepTracker>();
            }

            Pedometer = new Pedometer(stepTracker);

			m_logger.Debug("Setting compass type={0}", CompassType);

			switch (CompassType)
			{
			case CompassType.System:
				Compass = SystemCompass;
				break;
			case CompassType.NoiseReduced:
				Compass = NoiseReducedCompass;
				break;
			case CompassType.LocationTracker:				
				LocationTrackerCompass = new Motive.AR.LocationServices.LocationTrackerCompass();
				Compass = LocationTrackerCompass;
				break;
			case CompassType.Hybrid:
			default:
				var hybrid = new HybridCompass(NoiseReducedCompass);
				LocationTrackerCompass = hybrid.TrackerCompass;
				Compass = hybrid;
				break;
			}

            AudioChannel = CreateAudioPlayerChannel();
            ForegroundAudioChannel = gameObject.AddComponent<UnityAudioPlayerChannel>();

            Pedometer.Stepped += (sender, args) =>
            {
                if (ScriptEngine.Instance.UserInteractionEventManager != null)
                {
                    ScriptEngine.Instance.UserInteractionEventManager.AddEvent("step");
                }
            };

            if (ScreenSleepBehavior == ScreenSleepBehavior.AlwaysOn)
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }
        }

        /// <summary>
        /// Creates an audio player channel.
        /// </summary>
        /// <returns></returns>
        public IAudioPlayerChannel CreateAudioPlayerChannel()
        {
            IAudioPlayerChannel channel = null;

#if IOS_NATIVE
		if (Application.isMobilePlatform && EnableBackgroundAudio)
		{
			// TODO: the native channels need to be able to support streaming
			// loading, otherwise they block for too long.
			channel = new IOSAudioPlayerChannel();
		}
#endif
#if ANDROID_NATIVE
			if (Application.isMobilePlatform && EnableBackgroundAudio)
            {
                channel = new OpenSLAudioPlayerChannel();
            }
#endif

            if (channel == null)
            {
                channel = gameObject.AddComponent<UnityAudioPlayerChannel>();
            }

            return channel;
        }

        /// <summary>
        /// Creates an audio player using the default channel.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="allowBackground"></param>
        /// <returns></returns>
        public IAudioPlayer CreateAudioPlayer(string url, bool allowBackground = false)
        {
            var localUrl = WebServices.Instance.MediaDownloadManager.GetPathForItem(url);

            var player = AudioChannel.CreatePlayer(new Uri(localUrl));

            return new ManagedAudioPlayer(player, allowBackground);
        }

        /// <summary>
        /// Creates a spatial audio player channel for positional sound.
        /// </summary>
        /// <returns></returns>
        public ISpatialAudioPlayerChannel CreateSpatialAudioPlayerChannel()
        {
            ISpatialAudioPlayerChannel channel = null;

#if IOS_NATIVE_NOT_SUPPORTED
		if (Application.isMobilePlatform && EnableBackgroundAudio)
		{
			channel = new IOSAudioPlayerChannel();
		}
#endif

            if (channel == null)
            {
                channel = gameObject.AddComponent<UnitySpatialAudioPlayerChannel>();
            }

            return channel;
        }

        public void StartSensors()
		{
			m_logger.Debug("StartSensors");

            Pedometer.Start();
            Compass.Start();
        }
        
        void StartLiveSession()
        {
			m_logger.Debug("StartLiveSession: EnableBackgroundAudio={0}",
				EnableBackgroundAudio);

#if IOS_NATIVE
            if (Application.isMobilePlatform && EnableBackgroundAudio)
			{
                m_currentCategory = IOSAudioSessionCategory.Playback;

                IOSAudioSession.Instance.SetCategory(m_currentCategory, m_categoryOptions);
			}
#endif
            if (ScreenSleepBehavior == ScreenSleepBehavior.OnDuringLiveSessions)
            {
                ThreadHelper.Instance.CallOnMainThread(() =>
                {
                    Screen.sleepTimeout = SleepTimeout.NeverSleep;
                });
            }
        }

        void StopLiveSession()
		{
			m_logger.Debug("StopLiveSession: EnableBackgroundAudio={0} IsInBackground={1}",
				EnableBackgroundAudio, IsInBackground);

#if IOS_NATIVE
            if (EnableBackgroundAudio && Application.isMobilePlatform)
			{
                m_currentCategory = DefaultAudioCategory;

                IOSAudioSession.Instance.SetCategory(m_currentCategory, m_categoryOptions);
			}
#endif
            if (ScreenSleepBehavior == ScreenSleepBehavior.OnDuringLiveSessions)
            {
                ThreadHelper.Instance.CallOnMainThread(() =>
                {
                    Screen.sleepTimeout = SleepTimeout.SystemSetting;
                });
            }

			if (IsInBackground)
			{
				UserLocationService.Instance.Disable();

                if (AudioContentPlayer.Instance)
                {
                    AudioContentPlayer.Instance.Pause();
                }
            }
        }

        /// <summary>
        /// Suspend a live session.
        /// </summary>
		public void SuspendLiveSession()
		{
			IsLiveSessionSuspended = true;

			if (IsLiveSession)
			{
				StopLiveSession();
			}
		}

        /// <summary>
        /// Resume a previously suspended session.
        /// </summary>
		public void ResumeLiveSession()
		{
			if (IsLiveSession && IsLiveSessionSuspended)
			{
				IsLiveSessionSuspended = false;

				StartLiveSession();
			}
		}

        /// <summary>
        /// Requests a live session for the caller.
        /// </summary>
        /// <param name="listener"></param>
        public void StartLiveSession(object listener)
        {
			m_logger.Debug("StartLiveSession for {0} (count={1})", 
				listener.GetType().Name,
				m_liveSessionListeners.Count);

			lock (m_liveSessionListeners)
            {
				var callStart = (m_liveSessionListeners.Count == 0) && !IsLiveSessionSuspended;

                m_liveSessionListeners.Add(listener);

                if (callStart)
                {
                    StartLiveSession();
                }
            }
        }

        /// <summary>
        /// Stops a live session for the caller.
        /// </summary>
        /// <param name="listener"></param>
        public void StopLiveSession(object listener)
        {
			m_logger.Debug("StopLiveSession for {0} (count={1})", 
				listener.GetType().Name,
				m_liveSessionListeners.Count);

            lock (m_liveSessionListeners)
            {
                m_liveSessionListeners.Remove(listener);

                if (m_liveSessionListeners.Count == 0)
                {
                    StopLiveSession();
                }
            }
        }

        protected virtual void PauseAudio()
        {
            AudioChannel.Pause();
            LocativeAudioDriver.Instance.Stop();

            if (AudioContentPlayer.Instance)
            {
                AudioContentPlayer.Instance.Pause(immediate: true);
            }

            OnPauseAudio.Invoke();
        }

        protected virtual void ResumeAudio()
        {
            AudioChannel.Resume();

            if (AudioContentPlayer.Instance)
            {
                AudioContentPlayer.Instance.Resume(immediate: true);
            }

            LocativeAudioDriver.Instance.Start();

            OnResumeAudio.Invoke();
        }

        protected virtual void EnterBackground()
        {
            if (!IsInBackground)
            {
				m_logger.Debug("EnterBackground: IsLiveSession={0}", IsLiveSession);

                FireInteractionEvent("enterBackground");

                SetSystemState("runningInBackground");

                IsInBackground = true;
                OnEnterBackground.Invoke();

				if (!IsLiveSessionRunning)
                {
					// No live session: disable background location and audio.
					UserLocationService.Instance.Disable();

                    PauseAudio();

					#if IOS_NATIVE
                    if (Application.isMobilePlatform && EnableBackgroundAudio)
                    {
                        // On IOS, let's disable the audio session as well
                        try
                        {
                            IOSAudioSession.Instance.SetActive(false);
                        }
                        catch (Exception x)
                        {
                            // We don't want to halt if the native session trhows an error
                            m_logger.Exception(x);
                        }
                    }
					#endif
				}
				else
				{
					// If it is a live session, but these background modes are disabled,
					// shut down location and/or audio
					if (!EnableBackgroundLocation)
					{
						UserLocationService.Instance.Disable();
					}

					if (!EnableBackgroundAudio)
					{
                        PauseAudio();

						#if IOS_NATIVE
						// On IOS, let's disable the audio session as well
                        // We only use native iOS audio if background is enabled. This call 
                        // shouldn't be necessary.
						//IOSAudioSession.Instance.SetActive(false);
						#endif
					}
				}
            }
        }

        protected virtual void ExitBackground()
        {
            if (IsInBackground)
            {
				m_logger.Debug("ExitBackground");

                FireInteractionEvent("exitBackground");

                ClearSystemState("runningInBackground");

				#if IOS_NATIVE
				// On IOS, let's re-enable the audio session
                // Note: only use native iOS audio if background mode is
                // enabled.
                if (Application.isMobilePlatform && 
                    EnableBackgroundAudio &&
                    !IOSAudioSession.Instance.IsActive)
                {
                    // On IOS, let's disable the audio session as well
                    try
                    {
                        IOSAudioSession.Instance.SetActive(true);
                    }
                    catch (Exception x)
                    {
                        // We don't want to halt if the native session trhows an error
                        m_logger.Exception(x);
                    }
				}
				#endif		

                IsInBackground = false;
                OnExitBackground.Invoke();

                UserLocationService.Instance.Enable();

                ResumeAudio();
            }
        }

        /// <summary>
        /// Fires an interaction event that gets pushed into the Scripting system.
        /// </summary>
        /// <param name="eventName"></param>
        public void FireInteractionEvent(string eventName)
        {
            if (ScriptEngine.Instance.UserInteractionEventManager != null)
            {
                ScriptEngine.Instance.UserInteractionEventManager.AddEvent(eventName);
            }

        }

        void OnApplicationPause(bool isPaused)
        {
            FireInteractionEvent(isPaused ? "appPause" : "appResume");

            if (isPaused)
            {
                EnterBackground();
            }
            else
            {
                ExitBackground();
            }
        }

		void OnApplicationFocus(bool hasFocus)
		{
            FireInteractionEvent(hasFocus ? "appGotFocus" : "appLostFocus");

            if (!Application.isEditor)
            {
                if (hasFocus)
                {
                    ExitBackground();
                }
                else
                {
                    EnterBackground();
                }
            }
		}

		public void DownloadsComplete()
		{
			#if IOS_NATIVE
			if (Application.isMobilePlatform)
			{
				var folderName = System.IO.Path.Combine(AppDataPath, "downloads");

				//iPhone.SetNoBackupFlag(folderName);
				IOSFileDownloader.ExcludeFromBackup(folderName);
			}
			#endif
		}

        /// <summary>
        /// Plays a one-shot sound from a Motive resource.
        /// </summary>
        /// <param name="url"></param>
        public void PlaySound(string url)
        {
            var localUrl = WebServices.Instance.MediaDownloadManager.GetPathForItem(url);

			AudioChannel.Play(new Uri(localUrl));
        }

        /// <summary>
        /// Sets system state that Motive scripts can use.
        /// </summary>
        /// <param name="state"></param>
        public void SetSystemState(string state)
        {
            SystemStateManager.Instance.SetState("platform", state);
        }

        /// <summary>
        /// Clears a system state.
        /// </summary>
        /// <param name="state"></param>
        public void ClearSystemState(string state)
        {
            SystemStateManager.Instance.ClearState("platform", state);
        }

        /// <summary>
        /// Duck system sounds, for example when playing back narrator audio.
        /// </summary>
		public void DuckSystemSounds()
		{
			#if IOS_NATIVE
			if (Application.isMobilePlatform && 
                EnableBackgroundAudio &&
                MixWithOthers)
			{
				if (m_currentCategory == IOSAudioSessionCategory.PlayAndRecord ||
					m_currentCategory == IOSAudioSessionCategory.Playback)
				{
					m_categoryOptions |= 
						IOSAudioSessionCategoryOptions.InterruptSpokenAudioAndMixWithOthers |
						IOSAudioSessionCategoryOptions.DuckOthers;

					IOSAudioSession.Instance.SetCategory(m_currentCategory, m_categoryOptions);
				}
			}
			#endif
		}

        /// <summary>
        /// Unduck sounds, for example when a narrator audio track has completed.
        /// </summary>
		public void UnduckSystemSounds()
		{
			#if IOS_NATIVE
			if (Application.isMobilePlatform && 
                EnableBackgroundAudio &&
                MixWithOthers)
			{
				m_categoryOptions = MixWithOthers ?
					IOSAudioSessionCategoryOptions.MixWithOthers : IOSAudioSessionCategoryOptions.None;
				
				IOSAudioSession.Instance.SetCategory(m_currentCategory, m_categoryOptions);
			}
			#endif
		}
    }
}
