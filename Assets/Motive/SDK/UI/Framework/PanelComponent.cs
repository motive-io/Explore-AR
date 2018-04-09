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
        public Panel Panel { get; set; }

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

        public virtual void DidShow()
        {
            Populate();
        }

        public virtual void DidShow(object obj)
        {
            DidShow();
        }

        public virtual void DidHide()
        {
            Close();
        }

        public void SetText(Text text, string value)
        {
            if (text) text.text = value;
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

        public virtual void DidShow(T obj)
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
                ObjectHelper.SetObjectsActive(LinkedObjects, true);

                Data = (T)obj;
                Populate((T)obj);
            }
            else
            {
                ObjectHelper.SetObjectsActive(LinkedObjects, false);

				ClearData();
            }

            base.Populate(obj);
        }

        public override void DidShow(object obj)
        {
            if (obj is T)
            {
                DidShow((T)obj);
            }
			else
			{
				ObjectHelper.SetObjectsActive(LinkedObjects, false);

				ClearData();
			}

            base.DidShow(obj);
        }

        public void SetImage(GameObject layout, RawImage rawImage, string url)
        {
            if (layout)
            {
                layout.SetActive(url != null);
            }

            ImageLoader.LoadImageOnThread(url, rawImage);
        }

        public void SetText(Text textField, string value)
        {
            SetText(null, textField, value);
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
