// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive.UI.Framework
{
    /// <summary>
    /// The main PanelContainer for the app.
    /// </summary>
    public class PanelManager : PanelContainer
    {
        static PanelManager sInstance = null;

        public static PanelManager Instance
        {
            get { return sInstance; }
        }

        public void OpenUrl(string url)
        {
            Application.OpenURL(url);
        }

        protected override void Awake()
        {
            if (sInstance != null)
            {
                Debug.LogError("SingletonComponent.Awake: error " + name + " already initialized");
            }

            sInstance = this;

            base.Awake();
        }
    }
}