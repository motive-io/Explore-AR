// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.WebServices;
using Motive.Unity.Android;
using Motive.Unity.Utilities;
using UnityEngine;
using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive.Unity
{
    public class PushNotificationManager : SingletonComponent<PushNotificationManager>
    {
        const string GoogleCloud = "GCM";
        const string Apple = "APNS";
        const string AppleSandbox = "APNS_SANDBOX";

        Logger m_logger;

        //project id for android gcm
        public string GoogleConsoleProjectId;

        protected override void Awake()
        {
            base.Awake();

            m_logger = new Logger(this);
        }

        bool didRegister = false;

        void Update()
        {
            if (!didRegister && MotiveAuthenticator.Instance.IsUserAuthenticated)
            {
                didRegister = true;
                RegisterDevice();
            }
        }

#if UNITY_IOS
    int count = 0;
    void CheckForDeviceToken()
    {
        var token = UnityEngine.iOS.NotificationServices.deviceToken;
        var error = UnityEngine.iOS.NotificationServices.registrationError;

        if (count >= 100 || !string.IsNullOrEmpty(error))
        {
            CancelInvoke("CheckForDeviceToken");
            m_logger.Debug("Cancel polling");
            return;
        }

        m_logger.Debug("CheckForDeviceToken: got {0} (err:{1})", token, error);

        if (token != null)
        {
            CancelInvoke("CheckForDeviceToken");

            var tokenStr = System.BitConverter.ToString(token).Replace("-", "");

            var platform = BuildSettings.IsDebug ? AppleSandbox : Apple;

            WebServices.Instance.UserManager.RegisterPushNotifications(platform, tokenStr, (success) =>
            {
                m_logger.Debug("Register iOS push notifications {0} complete: success={1}", platform, success);
            });
        }

        count++;
    }

	void RegisterIOS()
	{
		UnityEngine.iOS.NotificationServices.RegisterForNotifications(
			UnityEngine.iOS.NotificationType.Alert |
			UnityEngine.iOS.NotificationType.Badge |
			UnityEngine.iOS.NotificationType.Sound, true);

		CancelInvoke("CheckForDeviceToken");
		InvokeRepeating("CheckForDeviceToken", 1f, 1f);
	}
#endif

        public void RegisterDevice()
        {
            if (Application.isMobilePlatform)
            {
#if UNITY_ANDROID
                RegisterAndroid();
#elif UNITY_IOS
            RegisterIOS();
#endif
            }
        }

        void WaitForUpdates()
        {
            MotiveAuthenticator.Instance.Updated += Auth_Updated;
        }

        // Re-register any time the authentication state updates.
        void Auth_Updated(object sender, MotiveAuthenticatorEventArgs e)
        {
            RegisterDevice();
        }

#if UNITY_ANDROID
        void RegisterAndroid()
        {
            m_logger.Debug("RegisterAndroid");

            if (string.IsNullOrEmpty(GoogleConsoleProjectId))
            {
                m_logger.Warning("Google project ID not configured.");

                return;
            }

            AndroidRemoteNotificationManager.Register(GoogleConsoleProjectId, (regId) =>
            {
                if (string.IsNullOrEmpty(regId))
                {
                    m_logger.Error("Failed to get registration ID");

                //ResultText.text = string.Format("Failed to get the registration id");
                return;
                }

            //ResultText.text = string.Format(@"Your registration Id is = {0}", regId);
            m_logger.Debug("in Register method");

                WebServices.Instance.UserManager.RegisterPushNotifications(GoogleCloud, regId, (success) =>
                {
                    m_logger.Debug("Register Android push notifications complete: success={0}", success);
                });

                WaitForUpdates();
            });
        }
#endif
    }

}