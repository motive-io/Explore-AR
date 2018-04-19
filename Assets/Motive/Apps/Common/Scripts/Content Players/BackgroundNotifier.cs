// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Notifications;
using Motive.Unity.Apps;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive.Unity.Playables
{
    public class BackgroundNotifier : SingletonComponent<BackgroundNotifier>
    {
        private Dictionary<string, LocalNotification> m_notifications;

        Logger m_logger;

        protected override void Awake()
        {
            m_logger = new Logger(this);

            AppManager.Instance.Initialized += (sender, args) =>
            {
                Initialize();
            };

            base.Awake();
        }

        public void Initialize()
        {
            try
            {
                Platform.Instance.LocalNotificationManager.CancelAllNotifications();
                m_notifications = new Dictionary<string, LocalNotification>();
            }
            catch (Exception e)
            {
                m_logger.Debug("Background Notifier failed to initialize.: \n{0}", e.Message);
            }
        }

        public bool IsExpired(LocalNotification notification)
        {
            if (!notification.SendTime.HasValue) return false;

            return notification.SendTime.Value < DateTime.Now;
        }

        public bool HasNotification(string id)
        {
            lock (m_notifications)
            {
                return m_notifications.ContainsKey(id);
            }
        }

        public void RemoveNotification(string id)
        {
            lock (m_notifications)
            {
                m_notifications.Remove(id);
            }
        }

        public void PutNotification(string id, LocalNotification notification)
        {
            m_logger.Debug("SetNotification: {0}", id);

            lock (m_notifications)
            {
                m_notifications[id] = notification;
            }

            if (Platform.Instance.IsInBackground)
            {
                SendToLocalNotificationManager(id, notification);
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
            m_logger.Debug("OnApplicationPause: {0}", isPaused);

            if (isPaused)
            {
                SendOutNotifications();
            }
            else
            {
                CancelAllSentNotifications();
            }
        }

        /*
        private void OnApplicationFocus(bool hasFocus)
        {
            m_logger.Debug("OnApplicationFocus: {0}", hasFocus);

            if (!hasFocus)
            {
                SendOutNotifications();
            }
            else
            {
                CancelAllSentNotifications();
            }
        }
        */

        private void SendToLocalNotificationManager(string id, LocalNotification notification)
        {
            m_logger.Debug("Sending notifications to android.");

            Platform.Instance.LocalNotificationManager.PostNotification(id, notification);
        }

        private void SendOutNotifications()
        {
            m_logger.Debug("SendOutNotifications");

            if (m_notifications == null) return;

            IEnumerable<KeyValuePair<string, LocalNotification>> kvs = null;

            lock (m_notifications)
            {
                kvs = m_notifications.Where(kv => !IsExpired(kv.Value));
            }

            foreach (var kv in kvs)
            {
                SendToLocalNotificationManager(kv.Key, kv.Value);
            }
        }

        private void CancelAllSentNotifications()
        {
            m_logger.Debug("CancelAllSentNotifications");

            if (Platform.Instance.LocalNotificationManager == null) return;

            Platform.Instance.LocalNotificationManager.CancelAllNotifications();
        }
    }

}