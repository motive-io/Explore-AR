// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Motive.Unity.Android
{
    /// <summary>
    /// Handles Android remote notifications.
    /// </summary>
    public class AndroidRemoteNotificationManager //: IRemoteNotificationManager
    {
        private const string CLASS_NAME = "com.google.android.gms.iid.InstanceID";

        public static void Register(string projectId, Action<string> OnRegisterCallback)
        {
            try
            {
                using (AndroidJavaClass cls = new AndroidJavaClass(CLASS_NAME))
                {
                    AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    AndroidJavaObject context = playerClass.GetStatic<AndroidJavaObject>("currentActivity");

                    var iid = cls.CallStatic<AndroidJavaObject>("getInstance", context);
                    var token = iid.Call<string>("getToken", projectId, "GCM");

                    Debug.Log("Android token: token = " + token);

                    if (OnRegisterCallback != null)
                        OnRegisterCallback(token);
                }
            }
            catch (Exception x)
            {
                Debug.Log("************* BANG! ****************");
                Debug.LogException(x);
            }
        }

        public static void Unregister()
        {

            if (Application.platform == RuntimePlatform.Android)
            {
                using (AndroidJavaClass cls = new AndroidJavaClass(CLASS_NAME))
                {
                    cls.CallStatic("unregister");
                }
            }
        }
    }

}