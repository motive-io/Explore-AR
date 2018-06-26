// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.UI.Models;
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Motive.Unity.World
{
    /// <summary>
    /// Manages object inspectors added to world objects.
    /// </summary>
    public class ObjectInspectorManager : SingletonComponent<ObjectInspectorManager>
    {

        public Panel ScreenMessagePanel;
        public Panel ScreenDialogPanel;

        ListDictionary<string, ResourceActivationContext> m_inspectors;
        SetDictionary<string, string> m_objectActions;
        HashSet<string> m_availableObjects;

        private string m_currAction;
        private string m_currObjId;
        private bool m_isOneShotAction;
        private string m_currInspectorId;
        private Panel m_currPanel;

        protected override void Awake()
        {
            m_inspectors = new ListDictionary<string, ResourceActivationContext>();

            m_availableObjects = new HashSet<string>();
            m_objectActions = new SetDictionary<string, string>();

            base.Awake();
        }

        void ApplyObjectAction(string objectId, string action, bool oneShot)
        {
            if (objectId == null)
            {
                return;
            }

            ResourceActivationContext inspectorCtxt = m_inspectors.GetLastOrDefault(objectId);

            if (inspectorCtxt != null && inspectorCtxt.InstanceId != m_currInspectorId)
            {
                // There's an inspector that matches this object
                m_currAction = action;
                m_currObjId = objectId;
                m_isOneShotAction = oneShot;

                var inspector = inspectorCtxt.Resource as ObjectInspector;

                if (inspector.Content is ScreenMessage)
                {
                    var screenMsg = (ScreenMessage)inspector.Content;

                    var data = new ResourcePanelData<ScreenMessage>(inspectorCtxt, screenMsg);

                    // Set curr id here so that a "pop" doesn't clear it.
                    m_currInspectorId = inspectorCtxt.InstanceId;

                    Action onClose = () =>
                    {
                        inspectorCtxt.Close();

                        m_currInspectorId = null;

                        if (m_isOneShotAction)
                        {
                            EndObjectAction(objectId, action);
                        }
                    };

                    if (screenMsg.Responses != null && screenMsg.Responses.Length > 0)
                    {
                        m_currPanel = ScreenDialogPanel;

                        PanelManager.Instance.Push(ScreenDialogPanel, data, onClose);
                    }
                    else
                    {
                        m_currPanel = ScreenMessagePanel;

                        PanelManager.Instance.Push(ScreenMessagePanel, data, onClose);
                    }
                }
            }
        }

        public void ObjectAction(string objectId, string action, bool oneShot)
        {
            if (action == m_currAction && objectId == m_currObjId)
            {
                return;
            }

            if (!oneShot)
            {
                m_objectActions.Add(objectId, action);
            }

            if (m_availableObjects.Contains(objectId))
            {
                ApplyObjectAction(objectId, action, oneShot);
            }
        }

        public void EndObjectAction(string objectId, string action)
        {
            m_objectActions.Remove(objectId, action);

            // For now ignore the action - ending any action removes this inspector.
            if (objectId == m_currObjId && m_currPanel != null)
            {
                PanelManager.Instance.Pop(m_currPanel);

                m_currAction = null;
                m_currObjId = null;
                m_currPanel = null;
            }
        }

        public void SetObjectAvailable(ResourceActivationContext sender, bool available)
        {
            if (available)
            {
                m_availableObjects.Add(sender.InstanceId);

                if (m_currObjId == sender.InstanceId)
                {
                    ApplyObjectAction(m_currObjId, m_currAction, m_isOneShotAction);
                }
            }
            else
            {
                m_availableObjects.Remove(sender.InstanceId);

                if (m_currObjId == sender.InstanceId)
                {
                    EndObjectAction(m_currObjId, m_currAction);
                }
            }
        }

        public void Select(ResourceActivationContext sender)
        {
            ObjectAction(sender.InstanceId, "select", true);
        }

        public void Select(string objectId)
        {
            ResourceActivationContext inspectorCtxt = null;

            if (m_inspectors.Keys.Contains(objectId))
            {
                inspectorCtxt = m_inspectors.GetFirstOrDefault(objectId);
                if (inspectorCtxt != null)
                {
                    Select(inspectorCtxt);
                }
            }
        }

        public void ObjectAction(ResourceActivationContext sender, string action, bool oneShot = false)
        {
            ObjectAction(sender.InstanceId, action, oneShot);
        }

        public void EndObjectAction(ResourceActivationContext sender, string action)
        {
            EndObjectAction(sender.InstanceId, action);
        }

        public void ActivateInspector(ResourceActivationContext context, ObjectInspector inspector)
        {
            if (inspector.TargetReferences != null)
            {
                string targetId = null;
                string targetAction = null;

                foreach (var reference in inspector.TargetReferences)
                {
                    var id = context.GetInstanceId(reference.ObjectId);

                    if (targetId == null && m_objectActions.GetCount(id) > 0)
                    {
                        // TODO: inspectors should define this action
                        targetAction = m_objectActions[id].First();
                        targetId = id;
                    }

                    m_inspectors.Add(id, context);
                }

                // No current object or current object is the same as targetId
                if (targetId != null)
                {
                    if (m_currObjId == null ||
                        m_currObjId == targetId)
                    {
                        // Actions stored in m_objectActions are never one-shot
                        ApplyObjectAction(targetId, targetAction, false);
                    }
                }
            }
        }

        public void DeactivateInspector(ResourceActivationContext context, ObjectInspector inspector)
        {
            if (inspector.TargetReferences != null)
            {
                foreach (var reference in inspector.TargetReferences)
                {
                    var id = context.GetInstanceId(reference.ObjectId);

                    m_inspectors.Remove(id, context);
                }
            }
        }
    }
}