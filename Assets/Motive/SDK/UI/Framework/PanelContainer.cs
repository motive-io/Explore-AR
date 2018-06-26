// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using Logger = Motive.Core.Diagnostics.Logger;
using Motive.Unity.Utilities;

namespace Motive.UI.Framework
{
    /// <summary>
    /// Defines how panels behave in this container's stacks.
    /// </summary>
    public enum PanelStackBehavior
    {
        /// <summary>
        /// Normal push/pop semantics.
        /// </summary>
        Normal,
        /// <summary>
        /// Panels are only shown one at a time. When a new panel is pushed,
        /// the currently shown panel is popped.
        /// </summary>
        OneAtATime,
        /// <summary>
        /// Panels do not use stacks and are shown/hidden independently from each other.
        /// </summary>
        NoStacks
    }

    /// <summary>
    /// Represents a collection of panels.
    /// </summary>
    public class PanelContainer : MonoBehaviour
    {
        protected class PanelStack
        {
            internal List<Panel> m_panels;
            internal Panel m_homePanel;
            internal string m_stackName;

            internal Panel CurrentPanel
            {
                get
                {
                    var p = m_panels.LastOrDefault();

                    if (p == null)
                    {
                        p = m_homePanel;
                    }

                    return p;
                }
            }

            internal PanelStack(string stackName, Panel home = null)
            {
                m_stackName = stackName;
                m_homePanel = home;

                m_panels = new List<Panel>();
            }

            internal void Remove(Panel p)
            {
                m_panels.Remove(p);
            }

            internal void Pop()
            {
                if (m_panels.Count > 0)
                {
                    m_panels.RemoveAt(m_panels.Count - 1);
                }
            }
        };

        Dictionary<string, PanelStack> m_namedStacks;
        Dictionary<Type, Panel> m_panelTypeDict;
        Dictionary<string, Panel> m_panelNameDict;
        Dictionary<Panel, PanelStack> m_pushedPanelStacks;
        HashSet<Panel> m_showPanels;
        public ScreenOrientation DefaultOrientation { get; private set; }
        Logger m_logger;

        //public bool UseStacks = true;
        public Panel RootPanel;

        public PanelStackBehavior StackBehavior;

        /// <summary>
        /// If true, attaches any panels beneath this container's GameObject
        /// in the hierarchy.
        /// </summary>
        public bool AttachAllChildPanels = false;
        public string DefaultStackName = "main";

        public GameObject[] ShowWhenPanelsShowing;
        public GameObject[] ShowWhenNoPanelsShowing;

        List<PanelStack> m_panelStacks;

        PanelStack CurrentPanelStack
        {
            get { return m_panelStacks.LastOrDefault(); }
        }

        //        List<Panel> m_panelStack;
        //		List<Panel> m_homePanelStack;

        public Panel CurrentPanel
        {
            get
            {
                var currStack = CurrentPanelStack;

                if (currStack != null)
                {
                    return currStack.CurrentPanel;
                }

                return null;
            }
        }

        protected virtual void Awake()
        {
            DefaultOrientation = Screen.orientation;

            m_panelStacks = new List<PanelStack>();
            var mainStack = new PanelStack(DefaultStackName, RootPanel);

            m_panelStacks.Add(mainStack);
            m_namedStacks = new Dictionary<string, PanelStack>
            {
                { DefaultStackName, mainStack }
            };
            m_pushedPanelStacks = new Dictionary<Panel, PanelStack>();
            m_showPanels = new HashSet<Panel>();

            m_logger = new Logger(this);

            var allPanels = GetComponentsInChildren<Panel>(true);

            m_panelTypeDict = new Dictionary<Type, Panel>();
            m_panelNameDict = new Dictionary<string, Panel>();

            foreach (var panel in allPanels)
            {
                m_panelTypeDict[panel.GetType()] = panel;
                m_panelNameDict[panel.gameObject.name.TrimEnd()] = panel;

                if (AttachAllChildPanels && !panel.Container)
                {
                    panel.Container = this;
                }
            }

            ObjectHelper.SetObjectsActive(ShowWhenPanelsShowing, false);
            ObjectHelper.SetObjectsActive(ShowWhenNoPanelsShowing, true);
        }

        protected virtual void Start()
        {
            if (RootPanel)
            {
                PushStack(CurrentPanelStack);
            }
        }

        void SetPanelActive(Panel p, bool active)
        {
            p.SetActive(active);
        }

        void PopPanel(PanelStack panelStack, Panel p, bool animate = true, RuntimeAnimatorController animController = null)
        {
            bool isTop = (p == panelStack.CurrentPanel);

            // Can only call PopPanel on the top panel
            /* rc - 11/28/2017: let panels get "popped" from
             * the middle of a stack. Most of the time the opposite
             * behaviour is unwanted (a middle panel taking anything that was
             * pushed after with it)
            if (p != panelStack.CurrentPanel)
            {
                return;
            }*/

            AnalyticsHelper.FireEvent("PopPanel", new Dictionary<string, object>
                {
                    { "Name", p.GetType().ToString() }
                });

            m_logger.Debug("PopPanel {0} ({1}) stack={2} start", p.name, p.GetType().Name, panelStack.m_stackName);

            panelStack.Remove(p);

            m_pushedPanelStacks.Remove(p);
            m_showPanels.Remove(p);

            var curr = panelStack.CurrentPanel;

            var onClose = p.OnClose;
            p.OnClose = null;

            if (onClose != null)
            {
                onClose();
            }

            if (!isTop)
            {
                // Skip animations, etc. The code
                // after this conditional is only
                // applicable when popping the top
                // panel
                SetPanelActive(p, false);

                p.DidPop();

                return;
            }

            // If the current top panel is the one that we remembered 
            // from before. This handles the case that "onClose" caused
            // the same panel to be pushed again (e.g. screen message ->
            // screen message)
            if (panelStack.CurrentPanel && (panelStack.CurrentPanel == curr))
            {
                m_logger.Verbose("PopPanel from {0} -> {1} is ready={2}",
                    p.GetType().Name, panelStack.CurrentPanel.GetType().Name, panelStack.CurrentPanel.IsReady);

                SetPanelActive(panelStack.CurrentPanel, true);
                panelStack.CurrentPanel.DidRegainTop();
            }

            SetPanelOrientation(curr);

            // Hide this panel last

            // We might be re-showing the same panel as part of the onClose flow,
            // if so, don't tell the panel it hid because this will shut the panel down.
            if (panelStack.CurrentPanel != p)
            {
                Action doPop = () =>
                {
                    var animCtl = animController ?? p.PopAnimation;

                    if (animCtl && animate)
                    {
                        p.DoAnimation(animCtl);
                    }
                    else
                    {
                        SetPanelActive(p, false);
                    }

                    p.DidPop();
                };

                if (panelStack.CurrentPanel && !panelStack.CurrentPanel.IsReady)
                {
                    m_logger.Verbose("Current panel {0} is not ready!", panelStack.CurrentPanel.name);

                    p.transform.SetAsLastSibling();

                    StartCoroutine(CallWhenReady(panelStack.CurrentPanel, () =>
                    {
                        // Make sure the panel we're disabling didn't become the
                        // current panel
                        if (p != panelStack.CurrentPanel)
                        {
                            doPop();
                        }
                    }));
                }
                else
                {
                    m_logger.Verbose("Current panel {0} is ready!", panelStack.CurrentPanel ? panelStack.CurrentPanel.name : null);

                    doPop();
                }
            }

            if (m_showPanels.Count == 0)
            {
                ObjectHelper.SetObjectsActive(ShowWhenPanelsShowing, false);
                ObjectHelper.SetObjectsActive(ShowWhenNoPanelsShowing, true);
            }

            m_logger.Verbose("PopPanel {0} end", p.GetType().Name);
        }

        void PopPanel(Panel p, bool animate = true, RuntimeAnimatorController animController = null)
        {
            if (p == CurrentPanelStack.m_homePanel)
            {
                PopStack();
                return;
            }

            PanelStack stack = null;

            if (m_pushedPanelStacks.TryGetValue(p, out stack))
            {
                PopPanel(stack, p, animate, animController);
            }
            else
            {
                m_logger.Error("Popping panel without a stack: {0}",
                    p.GetType().ToString(),
                    string.Join(",", m_pushedPanelStacks.Keys.Select(k => k.name).ToArray())
                );
            }
        }

        // This hack needs more thought, but works for now.
        // The goal is to reduce/eliminate the "flicker" between
        // screens.
        IEnumerator CallWhenReady(Panel toWatch, Action onReady)
        {
            while (!toWatch.IsReady)
            {
                yield return null;
            }

            m_logger.Verbose("{0} is ready", toWatch.name);

            onReady();
        }

        IEnumerator EnableWhenReady(Panel toEnable)
        {
            while (!toEnable.IsReady)
            {
                yield return null;
            }

            SetPanelActive(toEnable, true);
        }

        void PopTo(PanelStack stack, Panel p)
        {
            for (int i = stack.m_panels.Count - 1; i >= 0; i--)
            {
                if (p == stack.CurrentPanel)
                {
                    return;
                }

                var toPop = stack.m_panels[i];

                PopPanel(stack, toPop, (i == stack.m_panels.Count - 1), toPop.ExitAnimation ? toPop.ExitAnimation : toPop.PopAnimation);
            }
        }

        public void PopTo(Panel p)
        {
            PanelStack stack = null;

            if (p != null && m_pushedPanelStacks.TryGetValue(p, out stack))
            {
                PopTo(stack, p);
            }
            else
            {
                PopTo(CurrentPanelStack, p);
            }
        }

        public void Close(Panel p)
        {
            if (p.Container != null && p.Container != this)
            {
                p.Container.Close(p);
                return;
            }

            if (CurrentPanelStack.m_homePanel == p)
            {
                PopTo(CurrentPanelStack, null);

                PopStack();

                return;
            }

            PanelStack stack = null;

            if (m_pushedPanelStacks.TryGetValue(p, out stack))
            {
                PopTo(stack, null);
            }
            else
            {
                Hide(p, true, p.PopAnimation);
            }
        }

        /// <summary>
        /// Show a panel outside of a panel stack.
        /// </summary>
        /// <param name="p">panel</param>
        /// <param name="data">panel data</param>
        public void Show(Panel p, object data, Action onClose = null)
        {
            if (p.Container != null && p.Container != this)
            {
                p.Container.Show(p, data, onClose);
                return;
            }

            p.OnClose = onClose;

            DoShowPanel(p, true);

            p.DidPush(data);
        }

        public void Show(Panel p)
        {
            Show(p, null, null);
        }

        public T Show<T>(object data = null) where T : Panel
        {
            var panel = GetPanel<T>();

            if (panel)
            {
                Show(panel, data);
            }

            return panel;
        }

        /// <summary>
        /// Hide a panel outside of a stack. NOTE: this should only
        /// be called for panels that were previously Show-ed
        /// </summary>
        /// <param name="p">panel</param>
        public void Hide(Panel p, bool animate = true, RuntimeAnimatorController animController = null)
        {
            if (!m_showPanels.Contains(p))
            {
                // Make sure the panel is at least deactivated
                p.SetActive(false);

                return;
            }

            m_showPanels.Remove(p);

            m_pushedPanelStacks.Remove(p);

            var onClose = p.OnClose;
            p.OnClose = null;

            if (onClose != null)
            {
                onClose();
            }

            if (p.isActiveAndEnabled)
            {
                var animCtl = animController ?? p.PopAnimation;

                if (animCtl && animate)
                {
                    p.DoAnimation(animCtl);
                }
                else
                {
                    SetPanelActive(p, false);
                }

                p.DidPop();
            }

            if (m_showPanels.Count == 0)
            {
                ObjectHelper.SetObjectsActive(ShowWhenPanelsShowing, false);
                ObjectHelper.SetObjectsActive(ShowWhenNoPanelsShowing, true);
            }
        }

        public void Pop(Panel p)
        {
            Pop(p, false);
        }

        public void Pop(Panel p, bool popPanelsOnTop)
        {
            PanelStack stack = null;

            if (p.Container != null && p.Container != this)
            {
                p.Container.Pop(p);
                return;
            }

            if (m_pushedPanelStacks.TryGetValue(p, out stack))
            {
                if (popPanelsOnTop)
                {
                    PopTo(stack, p);
                }

                PopPanel(stack, p, true, p.PopAnimation);
            }
            else 
            {
                Hide(p, true, p.PopAnimation);
            }
        }

        public void Pop()
        {
            if (CurrentPanelStack != null)
            {
                var p = CurrentPanelStack.CurrentPanel;

                if (p != null)
                {
                    Pop(p);
                }
            }
        }

        public void PushSubPanel(Panel parent, Panel panel, object data = null, Action onClose = null)
        {
            Show(panel, data, onClose);
        }

        IEnumerator SetOrientationCoroutine(ScreenOrientation orientation)
        {
            if (orientation == ScreenOrientation.Unknown)
            {
                Screen.orientation = DefaultOrientation;
            }
            else
            {
                if (orientation == ScreenOrientation.AutoRotation)
                {
                    // Force the current device orientation, then switch to auto
                    switch (Input.deviceOrientation)
                    {
                        case DeviceOrientation.LandscapeLeft:
                            Screen.orientation = ScreenOrientation.LandscapeLeft;
                            break;
                        case DeviceOrientation.LandscapeRight:
                            Screen.orientation = ScreenOrientation.LandscapeRight;
                            break;
                        case DeviceOrientation.Portrait:
                            Screen.orientation = ScreenOrientation.Portrait;
                            break;
                        case DeviceOrientation.PortraitUpsideDown:
                            Screen.orientation = ScreenOrientation.PortraitUpsideDown;
                            break;
                    }

                    yield return null;

                    Screen.orientation = ScreenOrientation.AutoRotation;
                }
                else
                {
                    Screen.orientation = orientation;
                }
            }
        }

        public void SetOrientation(ScreenOrientation orientation)
        {
            if (gameObject.activeSelf && gameObject.activeInHierarchy)
            {
                StartCoroutine(SetOrientationCoroutine(orientation));
            }
        }

        void SetPanelOrientation(Panel p)
        {
            ScreenOrientation orientation = DefaultOrientation;

            if (p)
            {
                orientation = p.PreferredOrientation;
            }

            m_logger.Debug("Setting screen orientation={0} for screen={1}", orientation, p ? p.name : "none");

            SetOrientation(orientation);
        }

        void PushPanel(PanelStack stack, Panel p, object data, Action onClose, bool animate = true)
        {
            AnalyticsHelper.FireEvent("PushPanel", new Dictionary<string, object>
                {
                    { "Name", p.GetType().ToString() }
                });

            PanelStack currStack = null;

            m_pushedPanelStacks.TryGetValue(p, out currStack);

            if (currStack != null && currStack != stack)
            {
                m_logger.Error("Panel {0} already in another stack (curr={1} requested={2}), aborting push",
                    p.GetType().Name, currStack.m_stackName, stack.m_stackName);

                return;
            }

            m_pushedPanelStacks[p] = stack;

            m_logger.Debug("PushPanel {0} ({1}) stack={1} start", p.name, p.GetType().Name, stack.m_stackName);

            var curr = stack.CurrentPanel;

            if (curr == p)
            {
                // This panel is already showing, don't go any further.
                m_logger.Debug("PushPanel {0} stack={1} already on top", p.GetType().Name, stack.m_stackName);

                p.OnClose = onClose;

                p.DidPush(data);

                return;
            }

            if (curr)
            {
                m_logger.Verbose("PushPanel curr = {0}", curr.GetType().Name);

                var anim = curr.GetAnimationTarget().GetComponent<Animator>();

                if (anim && curr.IsReady)
                {
                    // Review this: if we remove the animator controller
                    // before it finishes its animation, we can end up
                    // in a situation where the panel is hidden or partially
                    // occluded.

                    // Only set it null if it's marked as "ready". This
                    // indicates that the animation has completed.
                    //anim.runtimeAnimatorController = null;
                    //Destroy(anim);
                    anim.enabled = false;
                }

                if (StackBehavior == PanelStackBehavior.Normal)
                {
                    curr.DidResignTop();

                    if (!curr.KeepActiveInStack)
                    {
                        SetPanelActive(curr, false);
                    }
                }
                else if (StackBehavior == PanelStackBehavior.OneAtATime)
                {
                    PopPanel(stack, curr);
                }
            }

            p.SetReady(false);

            // Remove this one from the stack if it's already there
            stack.m_panels.Remove(p);
            stack.m_panels.Add(p);

            p.OnClose = onClose;

            DoShowPanel(p, animate);

            p.DidPush(data);

            m_logger.Verbose("PushPanel {0} end", p.GetType().Name);
        }

        private void DoShowPanel(Panel p, bool animate)
        {
            SetPanelActive(p, true);

            SetPanelOrientation(p);

            m_showPanels.Add(p);

            if (m_showPanels.Count > 0)
            {
                ObjectHelper.SetObjectsActive(ShowWhenPanelsShowing, true);
                ObjectHelper.SetObjectsActive(ShowWhenNoPanelsShowing, false);
            }

            p.DoAnimation(animate ? p.PushAnimation : null);

            // It's up to the push animation to set the "ready"
            // flag. If no animation, set it directly.
            if (!p.PushAnimation)
            {
                p.SetReady(true);
            }
        }

        public void Push(Panel p, Action onClose = null, bool animate = true)
        {
            Push(p, null, onClose, animate);
        }

        public void Push(Panel p, object data, Action onClose = null, bool animate = true)
        {
            Push(p.StackName, p, data, onClose, animate);
        }

        public void Push(Panel p)
        {
            Push(p, null, null);
        }

        public Panel Swap(Panel currPanel, Panel newPanel, object data = null, Action onClose = null)
        {
            if (currPanel != null && currPanel != newPanel)
            {
                Pop(currPanel);
            }

            Push(newPanel, data, onClose);

            return newPanel;
        }

        public T Push<T>(string stackName, object data = null, Action onClose = null, bool animate = true)
            where T : Panel
        {
            var panel = GetPanel<T>();

            if (panel)
            {
                Push(stackName, panel, data, onClose, animate);
            }

            return panel;
        }

        public Panel Push(string panelName, object data = null, Action onClose = null, bool animate = true)
        {
            Panel panel = null;

            if (m_panelNameDict.TryGetValue(panelName, out panel))
            {
                Push(panel, data, onClose, animate);
            }

            return panel;
        }

        public void Push(string stackName, Panel p)
        {
            Push(stackName, p, null, null);
        }

        public void Push(string stackName, Panel p, object data, Action onClose = null, bool animate = true)
        {
            if (p.Container != null && p.Container != this)
            {
                p.Container.Push(stackName, p, data, onClose, animate);
                return;
            }

            PanelStack stack = null;

            if (CurrentPanelStack == null && string.IsNullOrEmpty(stackName))
            {
                stackName = DefaultStackName;
            }

            if (!string.IsNullOrEmpty(stackName))
            {
                m_namedStacks.TryGetValue(stackName, out stack);
            }
            else
            {
                stack = CurrentPanelStack;
            }

            if (stack == null)
            {
                AddStack(stackName, null);
                stack = m_namedStacks[stackName];
            }

            var curr = CurrentPanel;

            if (curr && curr.Modal)
            {
                // This prevents other panels from pre-empting the current one
                var call = curr.OnClose;

                curr.OnClose = () =>
                {
                    if (call != null)
                    {
                        call();
                    }

                    PushPanel(stack, p, data, onClose, animate);
                };
            }
            else
            {
                PushPanel(stack, p, data, onClose, animate);
            }
        }

        public void AddStack(string name, Panel homePanel)
        {
            var stack = new PanelStack(name, homePanel);
            m_namedStacks.Add(name, stack);
        }

        void PushStack(PanelStack stack, object data = null, Action onClose = null)
        {
            if (CurrentPanelStack != stack &&
                CurrentPanelStack.CurrentPanel)
            {
                CurrentPanelStack.CurrentPanel.DidResignTop();
                SetPanelActive(CurrentPanelStack.CurrentPanel, false);
            }

            if (stack.m_homePanel)
            {
                stack.m_homePanel.OnClose = onClose;
                SetPanelActive(stack.m_homePanel, true);
                stack.m_homePanel.SetReady(true);

                stack.m_homePanel.DidPush(data);
            }

            m_panelStacks.Add(stack);
        }

        public void PushStack(Panel homePanel = null, object data = null, Action onClose = null)
        {
            PanelStack stack = new PanelStack("<anonymous>", homePanel);

            PushStack(stack, data, onClose);
        }

        public void PushStack(string name, object data = null, Action onClose = null)
        {
            PanelStack stack = null;

            if (m_namedStacks.TryGetValue(name, out stack))
            {
                PushStack(stack, data, onClose);
            }
        }

        public void PushStack<T>(object data = null, Action onClose = null) where T : Panel
        {
            var p = GetPanel<T>();

            if (p)
            {
                PushStack(p, data, onClose);
            }
        }

        void PopStack(PanelStack stack)
        {
            m_panelStacks.Remove(stack);

            PopTo(stack, null);

            if (stack.m_homePanel)
            {
                stack.m_homePanel.DidPop();

                SetPanelActive(stack.m_homePanel, false);

                if (stack.m_homePanel.OnClose != null)
                {
                    stack.m_homePanel.OnClose();
                }
            }

            // Reactivate the top panel in the current stack stack
            if (CurrentPanel)
            {
                SetPanelActive(CurrentPanel, true);
            }
        }

        public void PopStack(string stackName)
        {
            PanelStack stack = null;

            if (m_namedStacks.TryGetValue(stackName, out stack))
            {
                PopStack(stack);
            }
        }

        public void PopStack()
        {
            if (m_panelStacks.Count == 1)
            {
                m_logger.Error("Trying to pop default stack, aborting.");
                return;
            }

            PopStack(CurrentPanelStack);
        }

        public Panel GetPanel(string name)
        {
            Panel p = null;

            m_panelNameDict.TryGetValue(name, out p);

            return p;
        }

        public T GetPanel<T>(string name)
            where T : Panel
        {
            return GetPanel(name) as T;
        }

        public T GetPanel<T>() where T : Panel
        {
            Panel p = null;

            m_panelTypeDict.TryGetValue(typeof(T), out p);

            return p as T;
        }

        public T Push<T>(object data, Action<T> onClose) where T : Panel
        {
            // This is a
            T p = GetPanel<T>();

            if (p != null)
            {
                Push(p, data, () =>
                {
                    onClose(p as T);
                });
            }

            return p;
        }

        public T Push<T>(object data = null, Action onClose = null, bool animate = true) where T : Panel
        {
            // This is a
            Panel p = null;

            if (m_panelTypeDict.TryGetValue(typeof(T), out p))
            {
                Push(p, data, onClose, animate);
            }

            return p as T;
        }

        internal void HideAll()
        {
            PopTo(CurrentPanelStack, null);
        }
    }
}