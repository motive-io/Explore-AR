// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.UI.Framework;
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using System;

public abstract class PlayableContentHandlerDelegate : MonoBehaviour
{
    public abstract void Preview(ResourceActivationContext activationContext, PlayableContent playable, DateTime playTime);
    public abstract void Play(ResourceActivationContext activationContext, PlayableContent playable, Action onClose);
    public abstract void StopPlaying(ResourceActivationContext activationContext, PlayableContent playable, bool interrupt);
    public abstract void Reset();
}
