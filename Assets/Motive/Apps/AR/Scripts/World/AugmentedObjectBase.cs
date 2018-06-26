// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Motive.Unity.AR
{
    public class AugmentedObjectBase : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent Clicked;

        public ARWorldObject WorldObject { get; set; }

        public bool IsSelectable;

        void Awake()
        {
            if (Clicked != null)
            {
                Clicked = new UnityEvent();
            }
        }
        
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (WorldObject != null)
            {
                WorldObject.OnClick();

                if (IsSelectable)
                {
                    ARWorld.Instance.Select(WorldObject);
                }
            }

            if (Clicked != null)
            {
                Clicked.Invoke();
            }
        }

        public virtual void OnFocus()
        {
            if (WorldObject != null)
            {
                WorldObject.OnFocus();
            }
        }

        public virtual void OnFocusLost()
        {
            if (WorldObject != null)
            {
                WorldObject.OnFocusLost();
            }
        }

        public virtual void OnGazeEnter()
        {
            if (WorldObject != null)
            {
                WorldObject.OnGazeEnter();
            }
        }

        public virtual void OnGazeExit()
        {
            if (WorldObject != null)
            {
                WorldObject.OnGazeExit();
            }
        }
    }
}
