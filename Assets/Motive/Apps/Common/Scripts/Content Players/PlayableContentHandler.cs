// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Core.Timing;
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using Motive.Unity.Scripting;
using System;
using System.Collections.Generic;
using Logger = Motive.Core.Diagnostics.Logger;
namespace Motive.Unity.Playables
{
    static class PlayableContentRoute
    {
        public const string Messages = "messages";
        public const string Screen = "screen";
        public const string Soundtrack = "soundtrack";
        public const string Narrator = "narrator";
        public const string Ambient = "ambient";
    }

    static class PlayableContentPriority
    {
        public const string Normal = "normal";
        public const string High = "high";
        public const string Interrupt = "interrupt";
        public const string Filler = "filler";
    }

    class BatchContext
    {
        public Timer Timer { get; set; }
        public bool Abort { get; set; }
        public ResourceActivationContext ActivationContext { get; set; }
        public PlayableContent[] Playables { get; set; }
    }

    /// <summary>
    /// This class handles playable timers and batches.
    /// </summary>
    public class PlayableContentHandler : Singleton<PlayableContentHandler>
    {
        public PlayableContentHandlerDelegate Delegate;

        Logger m_logger;
        Dictionary<string, BatchContext> m_containers;

        public PlayableContentHandler()
        {
            m_logger = new Logger(this);
            m_containers = new Dictionary<string, BatchContext>();

            ScriptManager.Instance.ScriptsReset += ScriptManager_ScriptsReset;
        }

        void ScriptManager_ScriptsReset(object sender, EventArgs e)
        {
            m_containers.Clear();

            if (Delegate)
            {
                Delegate.Reset();
            }
        }

        void Play(BatchContext context, PlayableContent playable, Action onClose)
        {
            if (!context.ActivationContext.IsClosed)
            {
                // If there's a script timer, use the activation time as a base to
                // play this content later.
                if (playable.Timer != null)
                {
                    // Use clock manager so that we can use debug clock features
                    var fireTime = playable.Timer.GetNextFireTime(context.ActivationContext.ActivationTime);
                    var delta = ClockManager.Instance.GetTimespanFromNow(fireTime);

                    if (delta.TotalSeconds > 0)
                    {
                        // Actual play time is real Now + delta - this can be different from the 
                        // computed fire time above if we're using a debug clock.
                        var actualPlayTime = DateTime.Now + delta;

                        Delegate.Preview(context.ActivationContext, playable, actualPlayTime);
                    }

                    context.Timer = Timer.Call(delta,
                        () =>
                        {
                            m_logger.Debug("Fired timer!");

                            Delegate.Play(context.ActivationContext, playable, () =>
                            {
                                context.ActivationContext.Close();

                                if (onClose != null)
                                {
                                    onClose();
                                }
                            });
                        });
                }
                else
                {
                    Delegate.Play(context.ActivationContext, playable, () =>
                    {
                        context.ActivationContext.Close();

                        if (onClose != null)
                        {
                            onClose();
                        }
                    });
                }
            }
            else
            {
                if (onClose != null)
                {
                    onClose();
                }
            }
        }

        /// <summary>
        /// Play the specified playable. NOTE that this is called off-thread! Any
        /// interaction with Unity needs to use ThreadHelper.
        /// </summary>
        /// <param name="ctxt">Ctxt.</param>
        /// <param name="playable">Playable.</param>
        /// <param name="onClose">On close.</param>
        public void Play(ResourceActivationContext ctxt, PlayableContent playable, Action onClose = null)
        {
            m_logger.Debug("Play {0}", playable.Type);

            BatchContext playableContext = new BatchContext
            {
                ActivationContext = ctxt,
                Playables = new PlayableContent[] { playable }
            };

            m_containers[ctxt.InstanceId] = playableContext;

            Play(playableContext, playable, onClose);
        }

        void PlayNextFromBatch(BatchContext ctxt, PlayableContentBatch playableBatch, int idx)
        {
            if (idx < playableBatch.Playables.Length)
            {
                var playable = playableBatch.Playables[idx];

                Play(ctxt, playable, () =>
                {
                    if (!ctxt.Abort)
                    {
                        PlayNextFromBatch(ctxt, playableBatch, idx + 1);
                    }
                });
            }
            else
            {
                ctxt.ActivationContext.Close();
            }
        }

        public void Play(ResourceActivationContext ctxt, PlayableContentBatch playableBatch)
        {
            BatchContext playableContext = new BatchContext
            {
                ActivationContext = ctxt,
                Playables = playableBatch.Playables
            };

            m_containers[ctxt.InstanceId] = playableContext;

            if (playableBatch.Playables != null && playableBatch.Playables.Length > 0)
            {
                PlayNextFromBatch(playableContext, playableBatch, 0);
            }
            else
            {
                ctxt.Close();
            }
        }

        public void StopPlaying(ResourceActivationContext ctxt)
        {
            BatchContext context = null;

            if (m_containers.TryGetValue(ctxt.InstanceId, out context))
            {
                m_containers.Remove(ctxt.InstanceId);

                if (context.Timer != null)
                {
                    context.Timer.Cancel();
                }

                context.Abort = true;

                if (context.Playables != null)
                {
                    foreach (var playable in context.Playables)
                    {
                        Delegate.StopPlaying(ctxt, playable, false);
                    }
                }
            }
        }
    }

}