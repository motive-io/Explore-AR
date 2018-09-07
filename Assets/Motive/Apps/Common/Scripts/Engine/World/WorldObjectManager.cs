// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive._3D.Models;
using Motive.Core.Models;
using Motive.Core.Scripting;
using Motive.Core.Utilities;
using Motive.Unity.Models;
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive.Unity.World
{
    public class AppliedEffect
    {
        public IWorldObjectEffectHandler Handler { get; private set; }
        public WorldObjectBehaviour WorldObject { get; private set; }
        public ResourceActivationContext ActivationContext { get; private set; }
        public WorldObjectEffectPlayer EffectPlayer { get; private set; }
        public IScriptObject Effect { get; private set; }

        public AppliedEffect(IWorldObjectEffectHandler handler, WorldObjectBehaviour worldObject, ResourceActivationContext activationContext, WorldObjectEffectPlayer effectPlayer, IScriptObject effect)
        {
            Handler = handler;
            WorldObject = worldObject;
            ActivationContext = activationContext;
            EffectPlayer = effectPlayer;
            Effect = effect;
        }
    }

    public interface IWorldObjectEffectHandler
    {
        void RemoveEffect(AppliedEffect appliedEffect);
        void ApplyEffect(AppliedEffect appliedEffect);
    }

    public class WorldObjectManager : Singleton<WorldObjectManager>
    {
        private Dictionary<string, WorldObjectBehaviour> m_objectsByName;
        private SetDictionary<string, WorldObjectBehaviour> m_worldObjects;
        private ListDictionary<string, ActiveResourceContainer<WorldObjectEffectPlayer>> m_pendingEffects;
        private SetDictionary<string, AppliedEffect> m_appliedEffects;
        private Dictionary<string, IWorldObjectEffectHandler> m_effectHandlers;

        private Logger m_logger;

        public WorldObjectManager()
        {
            m_logger = new Logger(this);

            m_effectHandlers = new Dictionary<string, IWorldObjectEffectHandler>();
            m_objectsByName = new Dictionary<string, WorldObjectBehaviour>();
            m_worldObjects = new SetDictionary<string, WorldObjectBehaviour>();
            m_pendingEffects = new ListDictionary<string, ActiveResourceContainer<WorldObjectEffectPlayer>>();
            m_appliedEffects = new SetDictionary<string, AppliedEffect>();

            m_effectHandlers["motive.unity.animation"] = new UnityAnimationEffectHandler();
            m_effectHandlers["motive.3d.embeddedAnimation"] = new EmbeddedAnimationEffectHandler();
            m_effectHandlers["motive.3d.scriptedAnimation"] = new ScriptedAnimationHandler();
            m_effectHandlers["motive.3d.scriptedParticleEffect"] = new ScriptedParticleEffectHandler();
        }

        public void RegisterNamedWorldObject(MotiveSceneObject asset)
        {
            if (asset.Name != null)
            {
                m_objectsByName[asset.Name] = asset;
            }
        }

        public WorldObjectBehaviour GetWorldObject(string name)
        {
            if (m_objectsByName.ContainsKey(name))
            {
                return m_objectsByName[name];
            }

            return null;
        }

        public WorldObjectBehaviour GetWorldObject(ObjectReference assetRef)
        {
            if (assetRef != null &&
                assetRef.ObjectId != null &&
                m_worldObjects.GetCount(assetRef.ObjectId) > 0)
            {
                return m_worldObjects[assetRef.ObjectId].First();
            }
            else
            {
                // Let's see if this matches a named object
                var worldObj = NamedWorldObjectDirectory.Instance.GetItem(assetRef);

                if (worldObj != null &&
                    m_objectsByName.ContainsKey(worldObj.Name))
                {
                    return m_objectsByName[worldObj.Name];
                }
            }

            return null;
        }

        void AddSpawnedAsset(ResourceActivationContext context, AssetSpawner resource, UnityAsset asset, GameObject obj)
        {
            GameObject rootObject = new GameObject(asset.AssetName);

            rootObject.transform.position = Vector3.zero;
            rootObject.transform.localScale = Vector3.one;
            rootObject.transform.rotation = Quaternion.Euler(Vector3.zero);

            var worldObj = AddWorldObject(context, obj, rootObject);

            worldObj.Asset = asset;

            obj.transform.position = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.rotation = Quaternion.Euler(Vector3.zero);

            obj.transform.SetParent(rootObject.transform);

            if (resource.SpawnPosition is FixedWorldPosition)
            {
                var fixedPos = resource.SpawnPosition as FixedWorldPosition;

                if (fixedPos.Position != null)
                {
                    rootObject.transform.position = LayoutHelper.ToVector3(fixedPos.Position);
                }

                if (resource.AssetInstance.Layout != null)
                {
                    LayoutHelper.Apply(obj.transform, resource.AssetInstance.Layout);
                }
            }

            m_worldObjects.Add(resource.AssetInstance.Id, worldObj);
        }

        public IEnumerable<WorldObjectBehaviour> GetWorldObjects(string instanceId)
        {
            return m_worldObjects[instanceId];
        }

        public WorldObjectBehaviour AddWorldObject(ResourceActivationContext context, GameObject obj, GameObject animationTarget, GameObject rootObj = null)
        {
            context.Open();

            var worldObj = (rootObj ?? obj).AddComponent<WorldObjectBehaviour>();

            worldObj.AnimationTarget = animationTarget;
            worldObj.InteractibleObject = obj;
            worldObj.WorldObject = obj;
            worldObj.ActivationContext = context;

            m_worldObjects.Add(context.InstanceId, worldObj);

            var effects = m_pendingEffects[context.InstanceId];

            m_pendingEffects.RemoveAll(context.InstanceId);

            if (effects != null)
            {
                foreach (var container in effects)
                {
                    ApplyEffects(worldObj, container.ActivationContext, container.Resource);
                }
            }

            return worldObj;
        }

        public void RemoveWorldObjects(string instanceId)
        {
            m_worldObjects.RemoveAll(instanceId);

            var effects = m_appliedEffects[instanceId];

            if (effects != null)
            {
                foreach (var effect in effects)
                {

                }
            }
        }

        public void SpawnAsset(ResourceActivationContext context, AssetSpawner resource)
        {
            if (resource.AssetInstance == null ||
                resource.AssetInstance.Asset == null)
            {
                return;
            }

            var asset = resource.AssetInstance.Asset as UnityAsset;

            if (asset != null)
            {
                if (asset.AssetBundle != null)
                {
                    AssetLoader.LoadAsset<GameObject>(asset, obj =>
                    {
                        if (obj)
                        {
                            AddSpawnedAsset(context, resource, asset, obj);
                        }
                    });
                }
                else
                {
                    var obj = Resources.Load<GameObject>(asset.AssetName);

                    if (obj)
                    {
                        AddSpawnedAsset(context, resource, asset, GameObject.Instantiate(obj));
                    }
                }
            }

            context.Open();
        }

        public void UnspawnAsset(AssetSpawner resource)
        {
            var objs = m_worldObjects[resource.Id];

            if (objs != null)
            {
                foreach (var inst in objs)
                {
                    if (inst && inst.gameObject)
                    {
                        GameObject.Destroy(inst.gameObject);
                    }
                }

                m_worldObjects.RemoveAll(resource.Id);
            }
        }

        public void RemoveEffects(ResourceActivationContext context, WorldObjectEffectPlayer resource)
        {
            var appliedEffects = m_appliedEffects[context.InstanceId];

            if (appliedEffects != null)
            {
                foreach (var effect in appliedEffects)
                {
                    effect.Handler.RemoveEffect(effect);
                }
            }

            if (resource.WorldObjectReferences != null)
            {
                foreach (var objRef in resource.WorldObjectReferences)
                {
                    var objInstId = context.GetInstanceId(objRef.ObjectId);

                    m_pendingEffects.RemoveAll(objInstId);
                }
            }

            m_appliedEffects.RemoveAll(context.InstanceId);
        }

        void ApplyEffects(WorldObjectBehaviour worldObj, ResourceActivationContext effectContext, WorldObjectEffectPlayer resource)
        {
            if (resource.Effects != null)
            {
                foreach (var eref in resource.Effects)
                {
                    IWorldObjectEffectHandler handler;

                    if (m_effectHandlers.TryGetValue(eref.Type, out handler))
                    {
                        var appliedEffect = new AppliedEffect(handler, worldObj, effectContext, resource, eref);

                        handler.ApplyEffect(appliedEffect);

                        m_appliedEffects.Add(effectContext.InstanceId, appliedEffect);
                    }
                    else
                    {
                        var errStr = string.Format("Could not find handler for effect type {0}", eref.Type);

                        SystemErrorHandler.Instance.ReportError(errStr);

                        m_logger.Warning(errStr);
                    }
                }
            }
        }

        public void ApplyEffects(ResourceActivationContext context, WorldObjectEffectPlayer resource)
        {
            if (resource.WorldObjectReferences != null &&
                resource.Effects != null)
            {
                foreach (var wo in resource.WorldObjectReferences)
                {
                    var instId = context.GetInstanceId(wo.ObjectId);

                    var worldObjs = m_worldObjects[instId];

                    if (worldObjs != null)
                    {
                        foreach (var worldObj in worldObjs)
                        {
                            ApplyEffects(worldObj, context, resource);
                        }
                    }
                    else
                    {
                        m_pendingEffects.Add(instId, new ActiveResourceContainer<WorldObjectEffectPlayer>(context, resource));
                    }
                }
            }
        }
    }

}