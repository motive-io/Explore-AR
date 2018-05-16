// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Motive.UI.Framework
{
    /// <summary>
    /// Represents a named link between panels. Useful for connecting panels
    /// as prefabs.
    /// </summary>
    public class PanelLink : MonoBehaviour
    {
        public string PanelName;

        [SerializeField]
        Panel m_panel;

        public UnityEvent OnPush;
        public UnityEvent OnPop;

        public Panel GetPanel()
        {
            if (m_panel == null)
            {
                m_panel = PanelManager.Instance.GetPanel(PanelName);

                if (m_panel == null)
                {
                    Debug.LogWarningFormat("Could not find panel {0}", PanelName);
                }
            }

            return m_panel;
        }

        public T GetPanel<T>()
            where T : Panel
        {
            return GetPanel() as T;
        }

        public void Show()
        {
            Panel p = GetPanel();

            if (p != null)
            {
                PanelManager.Instance.Show(p, null, () =>
                {
                    if (OnPop != null)
                    {
                        OnPop.Invoke();
                    }
                });

                if (OnPush != null)
                {
                    OnPush.Invoke();
                }
            }
        }

        public void Push()
        {
            Panel p = GetPanel();

            if (p != null)
            {
                PanelManager.Instance.Push(p, null, () =>
                    {
                        if (OnPop != null)
                        {
                            OnPop.Invoke();
                        }
                    });

                if (OnPush != null)
                {
                    OnPush.Invoke();
                }
            }
        }

        public void Push(object data, Action onClose = null)
        {
            var p = GetPanel();

            if (p != null)
            {
                PanelManager.Instance.Push(p, data, () =>
                {
                    if (OnPop != null)
                    {
                        OnPop.Invoke();
                    }

                    if (onClose != null)
                    {
                        onClose();
                    }
                });

                if (OnPush != null)
                {
                    OnPush.Invoke();
                }
            }
        }

        public void Call(string methodName)
        {
            Panel p = GetPanel();

            if (p != null)
            {
                var method = p.GetType().GetMethod(methodName);

                if (method != null)
                {
                    method.Invoke(p, null);
                }
            }
        }

        public void Back()
        {
            Panel p = GetPanel();

            if (p != null)
            {
                p.Back();
            }
        }
    }
}
