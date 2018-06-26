// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Motive.Unity.Utilities
{
    /// <summary>
    /// Allows callers in a non-Unity thread to call into a Unity thread.
    /// </summary>
    public class ThreadHelper : MonoBehaviour
    {

        List<Action> m_mainThreadActions;
        HashSet<Action> m_mainThreadExclusiveActions;

        public static ThreadHelper Instance { get; private set; }

        int m_mainThreadId;

        void Awake()
        {
            Instance = this;

#if WINDOWS_UWP
            m_mainThreadId = Environment.CurrentManagedThreadId;
#else
            m_mainThreadId = Thread.CurrentThread.ManagedThreadId;
#endif

            /*
        int a, b;
        ThreadPool.GetMaxThreads(out a, out b);

        ThreadPool.SetMaxThreads(8, 8);*/
            m_mainThreadActions = new List<Action>();
            m_mainThreadExclusiveActions = new HashSet<Action>();
        }

        void CallAllActions()
        {
            if (m_mainThreadActions.Count > 0)
            {
                Action[] actions = null;

                lock (m_mainThreadActions)
                {
                    actions = m_mainThreadActions.ToArray();
                    m_mainThreadActions.Clear();
                }

                foreach (var action in actions)
                {
                    action();
                }
            }

            if (m_mainThreadExclusiveActions.Count > 0)
            {
                Action[] actions = null;

                lock (m_mainThreadExclusiveActions)
                {
                    actions = m_mainThreadExclusiveActions.ToArray();
                    m_mainThreadExclusiveActions.Clear();
                }

                foreach (var action in actions)
                {
                    action();
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            CallAllActions();
        }

        public bool IsUnityThread
        {
            get
            {
#if WINDOWS_UWP
                return Environment.CurrentManagedThreadId == m_mainThreadId;
#else
                return Thread.CurrentThread.ManagedThreadId == m_mainThreadId;
#endif
            }
        }

        public void CallExclusive(Action action)
        {
            if (IsUnityThread)
            {
                CallAllActions();
                action();
            }
            else
            {
                lock (m_mainThreadExclusiveActions)
                {
                    m_mainThreadExclusiveActions.Add(action);
                }
            }
        }

        public void CallOnMainThread(Action action)
        {
            if (IsUnityThread)
            {
                CallAllActions();
                action();
            }
            else
            {
                lock (m_mainThreadActions)
                {
                    m_mainThreadActions.Add(action);
                }
            }
        }
    }
}

