// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.AR.Models;
using Motive.Unity.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Motive.Unity.AR
{
    /// <summary>
    /// This class is used to place objects in AR World.
    /// </summary>
    public abstract class ARWorldObject
    {
        /// <summary>
        /// Options that define how to place the object in the AR world.
        /// </summary>
        public ILocationAugmentedOptions Options { get; set; }
        /// <summary>
        /// True if this object is currently visible in the AR view
        /// Note: does not take into account whether AR World is active.
        /// </summary>
        public bool IsVisible { get; private set; }
        /// <summary>
        /// True if this object is currently "focused" in the AR view.
        /// An object is focused if it is visible and it is the object
        /// closest to the center of the view.
        /// </summary>
        public bool IsInFocus { get; private set; }
        /// <summary>
        /// True if this object is currently being "gazed" at. An object
        /// is being gazed at if some part of it overlaps the center of the
        /// view. In case of multiple objects, the nearest one is being gazed
        /// at.
        /// </summary>
        public bool IsGazedAt { get; private set; }
        /// <summary>
        /// Game object to render in the AR view.
        /// </summary>
        public GameObject GameObject { get; set; }
        /// <summary>
        /// The actual object representing this World Object. May
        /// be different from GameObject (GameObject is usually a
        /// parent of TargetObject)
        /// </summary>
        public GameObject TargetObject { get; set; }
        /// <summary>
        /// Optional fixed offset to apply to the object.
        /// </summary>
        public Vector3? Offset { get; set; }

        /// <summary>
        /// Fired when the object is selected.
        /// </summary>
        public event EventHandler Selected;
        /// <summary>
        /// Fired when the object is selected.
        /// </summary>
        public event EventHandler Clicked;
        /// <summary>
        /// Fired when the object is deselected.
        /// </summary>
        public event EventHandler Deselected;
        /// <summary>
        /// Fired when the object is being "gazed" at
        /// </summary>
        public event EventHandler GazeEntered;
        /// <summary>
        /// Fired when the object is no longer being "gazed" at
        /// </summary>
        public event EventHandler GazeExited;
        /// <summary>
        /// Fired when the object is currently in focus
        /// </summary>
        public event EventHandler Focused;
        /// <summary>
        /// Fired when the object is no longer in focus
        /// </summary>
        public event EventHandler FocusLost;
        /// <summary>
        /// Fired when the object enters the visible area
        /// </summary>
        public event EventHandler EnteredView;
        /// <summary>
        /// Fired when the object enters the visible area
        /// </summary>
        public event EventHandler ExitedView;

        public virtual GameObject GetAnimationTarget()
        {
            if (TargetObject)
            {
                return TargetObject;
            }

            return GameObject;
        }

        /// <summary>
        /// Called when the object is selected.
        /// </summary>
        public virtual void OnSelect()
        {
            if (Selected != null)
            {
                Selected(this, EventArgs.Empty);
            }
        }

        public virtual void OnClick()
        {
            if (Clicked != null)
            {
                Clicked(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when the object is deselected.
        /// </summary>
        public virtual void OnDeselect()
        {
            if (Deselected != null)
            {
                Deselected(this, EventArgs.Empty);
            }
        }

        public virtual void OnFocus()
        {
            IsInFocus = true;

            if (Focused != null)
            {
                Focused(this, EventArgs.Empty);
            }
        }

        public virtual void OnFocusLost()
        {
            IsInFocus = false;

            if (FocusLost != null)
            {
                FocusLost(this, EventArgs.Empty);
            }
        }

        public virtual void OnGazeEnter()
        {
            IsGazedAt = true;

            if (GazeEntered != null)
            {
                GazeEntered(this, EventArgs.Empty);
            }
        }

        public virtual void OnGazeExit()
        {
            IsGazedAt = true;

            if (GazeExited != null)
            {
                GazeExited(this, EventArgs.Empty);
            }
        }

        public virtual void OnEnterView()
        {
            if (EnteredView != null)
            {
                EnteredView(this, EventArgs.Empty);
            }
        }

        public virtual void OnExitView()
        {
            if (ExitedView != null)
            {
                ExitedView(this, EventArgs.Empty);
            }
        }

        public virtual void SetVisible(bool visible)
        {
            if (visible != IsVisible)
            {
                IsVisible = visible;

                if (IsVisible)
                {
                    OnEnterView();
                }
                else
                {
                    OnExitView();
                }
            }
        }
    }
}