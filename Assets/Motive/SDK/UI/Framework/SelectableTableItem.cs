// Copyright (c) 2018 RocketChicken Interactive Inc.
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

        protected virtual void Awake()
        {
            if (OnSelected == null)
            {
                OnSelected = new UnityEvent();
            }
        }

        public virtual void Select()
        {
            if (OnSelected != null)
            {
                OnSelected.Invoke();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Selectable)
            {
                Select();
            }
        }
    }

}
