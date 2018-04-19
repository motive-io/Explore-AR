// Copyright (c) 2018 RocketChicken Interactive Inc.

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using Motive.Unity.Utilities;

namespace Motive.UI.Framework
{
    /// <summary>
    /// A panel that takes typed data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Panel<T> : Panel
    {
        public new T Data { get; private set; }

        public virtual void Populate(T data)
        {
        }

        public virtual void ClearData()
        {
        }

        public override void DidPush(object data)
        {
            m_dataObject = data;

            if (data is T)
            {
                Data = (T)data;
                Populate((T)data);
            }
            else
            {
                Data = default(T);

                ClearData();
            }

            base.DidPush(data);
        }
    }

    /// <summary>
    /// Represents a UI screen.
    /// </summary>
    public class Panel : MonoBehaviour
    {
        /// <summary>
        /// If set, this panel has been pre-positioned.
        /// </summary>
        public bool PrePositioned;
        /// <summary>
        /// If true, the panel should be activated at start.
        /// </summary>
        public bool ActiveAtStart;
        /// <summary>
        /// If true, the panel is a modal and can't be interrupted by other screens.
        /// </summary>
        public bool Modal;
        /// <summary>
        /// If true, the panel should stay active even if another panel is pushed over it.
        /// </summary>
        public bool KeepActiveInStack;
        /// <summary>
        /// Use this to separate your panels into different stacks. 
        /// </summary>
        public string StackName;

        public ScreenOrientation PreferredOrientation = ScreenOrientation.Unknown;

        /// <summary>
        /// A set of objects to activate/deactivate along with this panel.
        /// </summary>
        public GameObject[] LinkedObjects;

        /// <summary>
        /// Defines the target object for animations. If not set, use the object
        /// that the panel is attached to.
        /// </summary>
		public GameObject AnimationTarget;
        /// <summary>
        /// Optional push animation.
        /// </summary>
        public RuntimeAnimatorController PushAnimation;
        /// <summary>
        /// Optional pop animation.
        /// </summary>
        public RuntimeAnimatorController PopAnimation;
        /// <summary>
        /// Optional close animation.
        /// </summary>
        public RuntimeAnimatorController ExitAnimation;

        /// <summary>
        /// The container that this panel belongs to.
        /// </summary>
        public PanelContainer Container;

        public UnityEvent OnPush;
        public UnityEvent OnPop;

        public Action OnClose { get; set; }
        
        public bool IsReady { get; private set; }

        public object Data
        {
            get
            {
                return m_dataObject;
            }
        }

        protected object m_dataObject;
		bool m_isActive;
        IEnumerable<PanelComponent> m_components;

        protected virtual void Awake()
        {
            if (!PrePositioned)
            {
                var rect = transform as RectTransform;

                if (rect)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.anchoredPosition = Vector2.zero;
                }
            }

            m_components = GetComponents<PanelComponent>();

            foreach (var c in m_components)
            {
                c.Panel = this;
            }

			SetActive(ActiveAtStart || m_isActive);
        }

        public virtual void SetReady(bool isReady)
        {
            IsReady = isReady;

            if (isReady && !this.PrePositioned)
            {
                var rect = transform as RectTransform;

                if (rect)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.anchoredPosition = Vector2.zero;
                }
            }
        }

		public virtual GameObject GetAnimationTarget()
		{
            return (AnimationTarget != null) ? AnimationTarget : this.gameObject;
		}

        public virtual void DoAnimation(RuntimeAnimatorController animController)
        {
            var tgt = GetAnimationTarget();

            var anim = tgt.GetComponent<Animator>();

            if (animController)
            {
                if (tgt)
                {
                    if (!anim)
                    {
                        anim = tgt.AddComponent<Animator>();
                    }

                    if (anim)
                    {
                        anim.runtimeAnimatorController = animController;

                        anim.enabled = true;
                    }
                }
            }
            else
            {
                if (anim)
                {
                    anim.runtimeAnimatorController = null;
                }
            }
        }

        public virtual void SetActive(bool active)
        {
			m_isActive = active;

            gameObject.SetActive(active);

            ObjectHelper.SetObjectsActive(LinkedObjects, active);
        }

        public virtual void Populate()
        {
            //IsReady = true;
        }

        public virtual void PopulateComponent<T>(object data)
            where T : PanelComponent
        {
            var component = GetComponent<T>();

            if (component != null)
            {
                component.Populate(data);
            }
        }

        public virtual void PopulateComponents(object obj)
        {
            if (m_components != null)
            {
                foreach (var c in m_components)
                {
                    c.DidShow(obj);
                }
            }
        }

        public virtual void DidShow()
        {
            PopulateComponents(this.m_dataObject);
        }

        public virtual void DidHide()
        {
            if (LinkedObjects != null)
            {
                foreach (var obj in LinkedObjects)
                {
					if (obj)
					{
						obj.SetActive(false);
					}
                }
            }

            if (m_components != null)
            {
                foreach (var c in m_components)
                {
                    c.DidHide();
                }
            }
        }

        public virtual void DidResignTop()
        {
        }

        public virtual void DidRegainTop()
        {
        }

        public virtual void DidPush()
        {
            Populate();

            if (OnPush != null)
            {
                OnPush.Invoke();
            }

			DidShow();
        }

        public virtual void DidPush(object data)
        {
            m_dataObject = data;

            //IsReady = true;

            DidPush();
        }

        public virtual void DidPop()
        {
            //gameObject.BroadcastMessage("DidHide", SendMessageOptions.DontRequireReceiver);
            DidHide();

            if (OnPop != null)
            {
                OnPop.Invoke();
            }
        }

        public virtual void Close()
        {
			PanelManager.Instance.Close(this);
        }

        public virtual void Back()
        {
            PanelManager.Instance.Pop(this);
        }

        public virtual void PushPanel(PanelLink link)
        {
            link.Push(m_dataObject);
        }

        public virtual void PushPanel(Panel p)
        {
            PanelManager.Instance.Push(p, m_dataObject);
        }
    }
}
