// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Motive.UI.Framework
{
    /// <summary>
    /// A table item that can be selected by the user.
    /// </summary>
    public class SelectableTableItem : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent OnSelected;
        public bool Selectable = true;

        public GameObject EnabledWhenSelected;
        public GameObject EnabledWhenNotSelected;

        IEnumerable<PanelComponent> m_components;

        protected virtual void Awake()
        {
            if (OnSelected == null)
            {
                OnSelected = new UnityEvent();
            }

            m_components = GetComponents<PanelComponent>();
        }

        public virtual void Select()
        {
            if (OnSelected != null)
            {
                OnSelected.Invoke();
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (Selectable)
            {
                Select();
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
    }
}
