// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.UI.Framework
{
    /// <summary>
    /// Panel components allow panel logic to be broken out into smaller
    /// components.
    /// </summary>
    public class PanelComponent : MonoBehaviour
    {
        public virtual Panel Panel { get; set; }

        /// <summary>
        /// Pops the component's panel off the stack.
        /// </summary>
        public void Back()
        {
            if (Panel)
            {
                Panel.Back();
            }
        }

        public virtual void Populate()
        {

        }

        public virtual void Populate(object obj)
        {
            Populate();
        }

        /// <summary>
        /// Populates all components of the given type with the
        /// given data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public virtual void PopulateComponents<T>(object data)
            where T : PanelComponent
        {
            var components = GetComponents<T>();

            if (components != null)
            {
                foreach (var component in components)
                {
                    component.Populate(data);
                }
            }
        }

        /// <summary>
        /// Populates a component of the given type with the
        /// given data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public virtual void PopulateComponent<T>(object data)
            where T : PanelComponent
        {
            var component = GetComponent<T>();

            if (component != null)
            {
                component.Populate(data);
            }
        }

        /// <summary>
        /// Populates all components with the given data.
        /// </summary>
        /// <param name="data"></param>
        public virtual void PopulateComponents(object data)
        {
            var components = GetComponentsInChildren<PanelComponent>();

            foreach (var component in components)
            {
                if (component != this)
                {
                    component.Populate(data);
                }
            }
        }

        public virtual void Close()
        {

        }

        public virtual void DidPush()
        {
            Populate();
        }

        public virtual void DidPush(object obj)
        {
            DidPush();
        }

        public virtual void DidHide()
        {
            Close();
        }

        public void SetText(Text text, string value)
        {
            SetText(null, text, value);
        }

        public void SetText(GameObject layout, Text textField, string value)
        {
            if (textField)
            {
                textField.text = value;
            }

            if (layout)
            {
                layout.SetActive(!string.IsNullOrEmpty(value));
            }
        }
    }

    /// <summary>
    /// A panel component that takes typed data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PanelComponent<T> : PanelComponent
    {
        public GameObject[] LinkedObjects;

        public T Data { get; protected set; }

        protected virtual void Awake()
        {
        }

        public virtual void Populate(T obj)
        {
        }

        public virtual void DidPush(T obj)
        {
            Data = obj;
            Populate(obj);
        }

		public virtual void ClearData()
		{
		}

        public override void Populate(object obj)
        {
            if (obj is T)
            {
                ShowLinkedObjects();

                Data = (T)obj;
                Populate((T)obj);
            }
            /*
             * The linked objects get turned off on push, then get
             * progressively activated.
            else
            {
                HideLinkedObjects();

				ClearData();
            }*/

            base.Populate(obj);
        }

        protected virtual void HideLinkedObjects()
        {
            ObjectHelper.SetObjectsActive(LinkedObjects, false);
        }

        protected virtual void ShowLinkedObjects()
        {
            ObjectHelper.SetObjectsActive(LinkedObjects, true);
        }

        public override void DidPush(object obj)
        {
            if (obj is T)
            {
                DidPush((T)obj);
            }
			else
			{
				ObjectHelper.SetObjectsActive(LinkedObjects, false);

				ClearData();
			}

            base.DidPush(obj);
        }

        public void SetImage(GameObject layout, RawImage rawImage, string url)
        {
            if (layout)
            {
                layout.SetActive(url != null);
            }

            ImageLoader.LoadImageOnThread(url, rawImage);
        }
        
        public virtual void PushPanel(PanelLink link)
        {
            link.Push(Data);
        }

        public virtual void PushPanel(Panel p)
        {
            PanelManager.Instance.Push(p, Data);
        }
    }
}
