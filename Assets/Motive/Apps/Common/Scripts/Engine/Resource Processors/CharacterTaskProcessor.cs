// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Unity.Scripting;
using Motive.Gaming.Models;
using Motive.Core.Scripting;
using Motive.Unity.Gaming;

namespace Motive.Unity.Scripting
{
    public class CharacterTaskProcessor : ThreadSafeScriptResourceProcessor<CharacterTask>
    {
        public override void ActivateResource(ResourceActivationContext context, CharacterTask resource)
        {
            TaskManager.Instance.ActivateCharacterTask(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, CharacterTask resource)
        {
            TaskManager.Instance.DeactivateTask(context.InstanceId);
        }
    }
}