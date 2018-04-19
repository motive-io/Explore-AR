using UnityEngine;
using System.Collections;
using System;

namespace Motive.Unity.Apps
{
    public abstract class Initializer : MonoBehaviour
    {
        void Awake()
        {
            AppManager.Instance.Initialized += HandleInitialize;
        }

        private void HandleInitialize(object sender, EventArgs e)
        {
            Initialize();
        }

        protected abstract void Initialize();
    }
}