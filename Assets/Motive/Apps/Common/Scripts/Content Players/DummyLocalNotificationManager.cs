// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using UnityEngine;
using Motive.Core.Notifications;
using Motive.Unity.Utilities;

namespace Motive.Unity.Playables
{
    public class DummyLocalNotificationManager : ILocalNotificationManager
    {
        public void CancelAllNotifications() { }

        public void CancelNotification(string id) { }

        public void PostNotification(string id, Motive.Core.Notifications.LocalNotification notification) { }

        public void Vibrate()
        {
            ThreadHelper.Instance.CallOnMainThread(() =>
            {
                Handheld.Vibrate();
            });
        }
    }
}