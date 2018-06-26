// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive._3D.Models;
using Motive.Core.Models;
using Motive.Core.Scripting;
using Motive.Core.Utilities;
using Motive.Unity.Models;
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.World
{
    public class WorldObjectManager : Singleton<WorldObjectManager>
    {
        private Dictionary<string, WorldObjectBehaviour> m_objectsByName;
        private Dictionary<string, WorldObjectBehaviour> m_objectsById;
        private SetDictionary<string, WorldObjectBehaviour> m_worldObjects;
        private SetDictionary<string, ActiveResourceContainer<WorldObjectEffectPlayer>> m_pendingEffects;
        private SetDictionary<string, MonoBehaviour> m_effects;

        public WorldObjectManager()
        {
            m_objectsByName = new Dictionary<string, WorldObjectBehaviour>();
            m_objectsById = new Dictionary<string, WorldObjectBehaviour>();
            m_worldObjects = new SetDictionary<string, WorldObjectBehaviour>();
            m_pendingEffects = new SetDictionary<string, ActiveResourceContainer<WorldObjectEffectPlayer>>();
            m_effects = new SetDictionary<string, MonoBehaviour>();
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
                m_objectsById.ContainsKey(assetRef.ObjectId))
            {
                return m_objectsById[assetRef.ObjectId];
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

            m_objectsById[resource.AssetInstance.Id] = worldObj;
        }

        public IEnumerable<WorldObjectBehaviour> GetWorldObjects(string instanceId)
        {
            return m_worldObjects[instanceId];
        }

        public WorldObjectBehaviour AddWorldObject(ResourceActivationContext context, GameObject obj, GameObject animationTarget, GameObject rootObj = null)
        {
            var worldObj = (rootObj ?? obj).AddComponent<WorldObjectBehaviour>();

            worldObj.AnimationTarget = animationTarget;
            worldObj.InteractibleObject = obj;
            worldObj.WorldObject = obj;

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
            if (m_effects.ContainsKey(context.InstanceId))
            {
                var effects = m_effects[context.InstanceId];

                if (effects != null)
                {
                    foreach (var effect in effects)
                    {
                        GameObject.Destroy(effect);
                    }
                }
            }

            if (resource.WorldObjectReferences != null)
            {
                foreach (var objRef in resource.WorldObjectReferences)
                {
                    var instId = context.GetInstanceId(objRef.ObjectId);

                    m_pendingEffects.RemoveAll(instId);
                }
            }

            m_effects.RemoveAll(context.InstanceId);
        }

        void ApplyEffects(WorldObjectBehaviour worldObj, ResourceActivationContext effectContext, WorldObjectEffectPlayer resource)
        {
            if (resource.Effects != null)
            {
                foreach (var eref in resource.Effects)
                {
                    switch (eref.Type)
                    {
                        case "motive.unity.animation":

                            var anim = eref as UnityAnimation;

                            if (anim != null)
                            {
                                var asset = anim.Asset;

                                if (asset != null)
                                {
                                    var animTgt = worldObj.GetAnimationTarget();

                                    if (animTgt)
                                    {
                                        // Target may have been destroyed somewhere
                                        var objAnim = worldObj.GetAnimationTarget().AddComponent<WorldObjectAnimation>();
                                        objAnim.AnimationAsset = asset;

                                        m_effects.Add(effectContext.InstanceId, objAnim);
                                    }
                                }
                            }
                            break;
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