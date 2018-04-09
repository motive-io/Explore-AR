// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Models;
using Motive.Unity.Scripting;
using System;
using Motive.Core.Scripting;

namespace Motive.Unity.Scripting
{
    public class ScriptLauncherProcessor : ThreadSafeScriptResourceProcessor<ScriptLauncher>
    {

        public override void ActivateResource(ResourceActivationContext context, ScriptLauncher resource)
        {
            Action run = () =>
            {
                ScriptManager.Instance.LaunchScript(resource, context.FrameContext, context.InstanceId, (didClose) =>
                {
                    if (didClose)
                    {
                        context.Close();
                    }
                });
            };

            if (!context.IsClosed && resource.ScriptReference != null)
            {
                if (context.IsFirstActivation)
                {
                    ScriptManager.Instance.StopRunningScript(resource.ScriptReference.ObjectId, context.InstanceId, true, run);
                }
                else
                {
                    run();
                }
            }
        }

        public override void DeactivateResource(ResourceActivationContext context, ScriptLauncher resource, System.Action onComplete)
        {
            string runId = context.InstanceId;

            if (resource.ScriptReference != null)
            {
                ScriptManager.Instance.StopRunningScript(resource.ScriptReference.ObjectId, runId, false, onComplete);
            }
            else
            {
                if (onComplete != null)
                {
                    onComplete();
                }
            }
        }

        public override void ResetResource(ResourceActivationContext context, ScriptLauncher resource, System.Action onComplete)
        {
            string runId = context.InstanceId;

            if (resource.ScriptReference != null)
            {
                ScriptManager.Instance.StopRunningScript(resource.ScriptReference.ObjectId, runId, true, onComplete);
            }
            else
            {
                if (onComplete != null)
                {
                    onComplete();
                }
            }
        }
    }
}