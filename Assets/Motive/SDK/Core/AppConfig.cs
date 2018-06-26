// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity
{
    [CreateAssetMenu(fileName = "App Config", menuName = "Motive/App Config", order = 1)]
    public class AppConfig : ScriptableObject
    {
        public string ConfigName = "default";
        public string AppId;
        public string ApiKey;
        public string UserDomain;
    }
}
