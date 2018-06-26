// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Core.Models;
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.UI.Models;
using Motive.Unity.Scripting;
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive.Unity.Playables
{
    public delegate void PlayContentDelegate(
        ResourceActivationContext activationContext,
        PlayableContent playable,
        Action onClose);

    class QueuedPlayableContext
    {
        public ResourceActivationContext ActivationContext { get; set; }
        public PlayableContent Playable { get; set; }
        public Action OnClose { get; set; }
        public PlayContentDelegate OnPlay { get; set; }
    }

    public class PlayableContentQueue
    {
        List<QueuedPlayableContext> m_queuedPlayables;

        internal QueuedPlayableContext CurrentlyPlaying { get; private set; }

        internal DateTime LastPlayFinish { get; private set; }

        internal object SyncRoot { get { return m_queuedPlayables; } }

        internal PlayableContentQueue()
        {
            m_queuedPlayables = new List<QueuedPlayableContext>();
        }

        internal void Reset()
        {
            m_queuedPlayables.Clear();
        }

        internal void Add(QueuedPlayableContext queuedPlayableContext)
        {
            lock (m_queuedPlayables)
            {
                m_queuedPlayables.Add(queuedPlayableContext);
            }
        }

        internal void AddToFront(QueuedPlayableContext queuedPlayableContext)
        {
            lock (m_queuedPlayables)
            {
                m_queuedPlayables.Insert(0, queuedPlayableContext);
            }
        }

        internal QueuedPlayableContext GetNext()
        {
            lock (m_queuedPlayables)
            {
                if (m_queuedPlayables == null || m_queuedPlayables.Count == 0)
                {
                    CurrentlyPlaying = null;
                    return null;
                }
                else
                {
                    var ctxt = m_queuedPlayables[0];
                    m_queuedPlayables.RemoveAt(0);
                    CurrentlyPlaying = ctxt;
                    return ctxt;
                }
            }
        }

        internal void FinishedPlaying(QueuedPlayableContext ctxt)
        {
            lock (m_queuedPlayables)
            {
                if (ctxt == CurrentlyPlaying)
                {
                    LastPlayFinish = DateTime.Now;

                    CurrentlyPlaying = null;
                }
            }
        }

        internal void Remove(PlayableContent playable)
        {
            lock (m_queuedPlayables)
            {
                if (CurrentlyPlaying != null && CurrentlyPlaying.Playable == playable)
                {
                    LastPlayFinish = DateTime.Now;

                    CurrentlyPlaying = null;
                }

                m_queuedPlayables.RemoveAll((_ctxt) => _ctxt.Playable == playable);
            }
        }
    }

    public class DefaultPlayableContentHandlerDelegate : PlayableContentHandlerDelegate
    {
        public Panel ScreenMessagePanel;
        public Panel ScreenDialogPanel;
        public Panel CharacterMessagePanel;
        public Panel CharacterDialogPanel;
        public Panel NotificationPanel;

        public Panel VideoPanel;
        public Panel AudioPanel;
        public Panel ImagePanel;

        public string PanelStack = "main";

        public float MinFillerDelay = 15f;

        protected Logger m_logger;
        //List<QueuedPlayableContext> m_queuedContexts;
        //QueuedPlayableContext m_playingContext;
        PlayableContentQueue m_screenQueue;
        PlayableContentQueue m_narratorQueue;
        Dictionary<string, PlayableContentQueue> m_queues;
        LocativeAudioContentProcessor m_locativeAudioProcessor;

        protected virtual void Awake()
        {
            m_logger = new Logger(this);
            m_screenQueue = new PlayableContentQueue();
            m_narratorQueue = new PlayableContentQueue();
            m_queues = new Dictionary<string, PlayableContentQueue>();
            m_locativeAudioProcessor = new LocativeAudioContentProcessor();
        }

        private void Start()
        {
            PlayableContentHandler.Instance.Delegate = this;
        }

        public override void Reset()
        {
            m_screenQueue.Reset();
        }

        protected virtual Panel GetScreenMessagePanel(ResourceActivationContext context, PlayableContent playable)
        {
            return ScreenMessagePanel;
        }

        protected virtual Panel GetScreenDialogPanel(ResourceActivationContext context, PlayableContent playable)
        {
            return ScreenDialogPanel;
        }

        protected virtual Panel GetCharacterMessagePanel(ResourceActivationContext context, PlayableContent playable)
        {
            return CharacterMessagePanel;
        }

        protected virtual Panel GetCharacterDialogPanel(ResourceActivationContext context, PlayableContent playable)
        {
            return CharacterDialogPanel;
        }

        protected virtual Panel GetAudioPanel(ResourceActivationContext context, PlayableContent playable)
        {
            return AudioPanel;
        }

        protected virtual Panel GetImagePanel(ResourceActivationContext context, PlayableContent playable)
        {
            return ImagePanel;
        }

        protected virtual Panel GetVideoPanel(ResourceActivationContext context, PlayableContent playable)
        {
            return VideoPanel;
        }

        protected void Push(ResourceActivationContext context, string stackName, Panel panel, object data, Action onClose)
        {
            ThreadHelper.Instance.CallOnMainThread(() =>
            {
                context.Open();

                PanelManager.Instance.Push(stackName, panel, data, onClose);
            });
        }

        protected virtual void PushScreenDialogPanel(ResourceActivationContext context, PlayableContent playable, ScreenMessage screenMsg, ResourcePanelData<ScreenMessage> data, Action onClose)
        {
            Push(context, PanelStack, GetScreenDialogPanel(context, playable), data, onClose);
        }

        protected virtual void PushScreenMessagePanel(ResourceActivationContext context, PlayableContent playable, ScreenMessage screenMsg, ResourcePanelData<ScreenMessage> data, Action onClose)
        {
            Push(context, PanelStack, GetScreenMessagePanel(context, playable), data, onClose);
        }

        protected virtual void PushCharacterDialogPanel(ResourceActivationContext context, PlayableContent playable, CharacterMessage charMsg, ResourcePanelData<CharacterMessage> data, Action onClose)
        {
            Push(context, PanelStack, GetCharacterDialogPanel(context, playable), data, onClose);
        }

        protected virtual void PushCharacterMessagePanel(ResourceActivationContext context, PlayableContent playable, CharacterMessage charMsg, ResourcePanelData<CharacterMessage> data, Action onClose)
        {
            Push(context, PanelStack, GetCharacterMessagePanel(context, playable), data, onClose);
        }

        protected virtual void PushImagePanel(ResourceActivationContext context, PlayableContent playable, MediaContent media, ResourcePanelData<MediaContent> data, Action onClose)
        {
            Push(context, PanelStack, GetImagePanel(context, playable), data, onClose);
        }

        protected virtual void PushVideoPanel(ResourceActivationContext context, PlayableContent playable, MediaContent media, ResourcePanelData<MediaContent> data, Action onClose)
        {
            Push(context, PanelStack, GetVideoPanel(context, playable), data, onClose);
        }

        protected virtual void PushAudioPanel(ResourceActivationContext context, PlayableContent playable, MediaContent media, ResourcePanelData<MediaContent> data, Action onClose)
        {
            Push(context, PanelStack, GetAudioPanel(context, playable), data, onClose);
        }

        protected virtual void PlayMediaContent(ResourceActivationContext context, PlayableContent playable, MediaContent media, ResourcePanelData<MediaContent> data, Action onClose)
        {
            if (IsAudioContent(playable.Content))
            {
                PushAudioPanel(context, playable, media, data, onClose);
            }
            else if (IsImageContent(playable.Content))
            {
                PushImagePanel(context, playable, media, data, onClose);
            }
            else if (IsVideoContent(media))
            {
                PushVideoPanel(context, playable, media, data, onClose);
            }
            else
            {
                m_logger.Error("Unsupported content type with route=screen: {0}",
                    playable.Content == null ? "null" : playable.Content.Type);
            }
        }

        protected virtual void PlayScreenContent(ResourceActivationContext context, PlayableContent playable, Action onClose)
        {
            if (playable.Content is ScreenMessage)
            {
                var screenMsg = playable.Content as ScreenMessage;

                var data = new ResourcePanelData<ScreenMessage>(context, screenMsg);

                if (screenMsg.Responses != null && screenMsg.Responses.Length > 0)
                {
                    PushScreenDialogPanel(context, playable, screenMsg, data, onClose);
                }
                else
                {
                    PushScreenMessagePanel(context, playable, screenMsg, data, onClose);
                }
            }
            else if (playable.Content is CharacterMessage)
            {
                var charMsg = playable.Content as CharacterMessage;

                var data = new ResourcePanelData<CharacterMessage>(context, charMsg);

                if (charMsg.Responses != null && charMsg.Responses.Length > 0)
                {
                    PushCharacterDialogPanel(context, playable, charMsg, data, onClose);
                }
                else
                {

                    PushCharacterMessagePanel(context, playable, charMsg, data, onClose);
                }
            }
            else if (playable.Content is Notification)
            {
                var notification = playable.Content as Notification;

                var data = new ResourcePanelData<Notification>(context, notification);

                PlayNotification(context, playable, playable.Content as Notification, data, onClose);
            }
            else if (playable.Content is MediaContent)
            {
                var media = playable.Content as MediaContent;

                var data = new ResourcePanelData<MediaContent>(context, media);

                PlayMediaContent(context, playable, media, data, onClose);
            }
            else
            {
                m_logger.Error("Unsupported content type with route=screen: {0}",
                    playable.Content == null ? "null" : playable.Content.Type);
            }
        }

        protected virtual bool IsVideoContent(IScriptObject content)
        {
            var media = content as MediaContent;

            if (media != null &&
                media.MediaItem != null &&
                media.MediaItem.MediaType == Motive.Core.Media.MediaType.Video)
            {
                return true;
            }

            return false;
        }

        protected virtual bool IsImageContent(IScriptObject content)
        {
            var media = content as MediaContent;

            if (media != null &&
                media.MediaItem != null &&
                media.MediaItem.MediaType == Motive.Core.Media.MediaType.Image)
            {
                return true;
            }

            return false;
        }

        protected virtual bool IsAudioContent(IScriptObject content)
        {
            var media = content as MediaContent;

            if (media != null &&
                media.MediaItem != null &&
                media.MediaItem.MediaType == Motive.Core.Media.MediaType.Audio)
            {
                return true;
            }

            return false;
        }

        void PlayNextSequentialContent(PlayableContentQueue queue)
        {
            QueuedPlayableContext ctxt = null;

            lock (queue.SyncRoot)
            {
                if (queue.CurrentlyPlaying != null)
                {
                    return;
                }

                ctxt = queue.GetNext();
            }

            Action next = () =>
            {
                if (ctxt.OnClose != null)
                {
                    ctxt.OnClose();
                }

                lock (queue.SyncRoot)
                {
                    queue.FinishedPlaying(ctxt);
                }

                PlayNextSequentialContent(queue);
            };

            if (ctxt != null)
            {
                var playable = ctxt.Playable;

                if (ctxt.OnPlay != null)
                {
                    ctxt.OnPlay(ctxt.ActivationContext, playable, next);
                }
                else if (ctxt.OnClose != null)
                {
                    next();
                }
            }
        }

        protected PlayableContentQueue GetQueue(ResourceActivationContext activationContext, PlayableContent playable)
        {
            if (playable.Content is ScreenMessage || playable.Content is CharacterMessage)
            {
                return m_screenQueue;
            }

            return null;
        }

        protected virtual AudioContentRoute GetRouteForAudioContent(PlayableContent playable)
        {
            switch (playable.Route)
            {
                case PlayableContentRoute.Narrator:
                    return AudioContentRoute.Narrator;
                case PlayableContentRoute.Soundtrack:
                    return AudioContentRoute.Soundtrack;
                case PlayableContentRoute.Ambient:
                    return AudioContentRoute.Ambient;
            }

            return AudioContentRoute.Ambient;
        }

        protected virtual void BeforePlayAudio(ResourceActivationContext activationContext, PlayableContent playable, Action onComplete)
        {
            onComplete();
        }

        protected virtual void AfterPlayAudio(ResourceActivationContext activationContext, PlayableContent playable, Action onComplete)
        {
            onComplete();
        }

        public virtual void PlayAudioContent(ResourceActivationContext activationContext, PlayableContent playable, Action onClose)
        {
            activationContext.Open();

            AudioContentPlayer.Instance.PlayAudioContent(activationContext.InstanceId, playable.Content as LocalizedAudioContent, GetRouteForAudioContent(playable), (whenDone) =>
                {
                    BeforePlayAudio(activationContext, playable, whenDone);
                },
                () =>
                {
                    AfterPlayAudio(activationContext, playable, onClose);
                });
        }

        protected virtual bool IsScreenContent(PlayableContent playable)
        {
            if (playable.Content is ScreenMessage ||
                playable.Content is CharacterMessage ||
                playable.Content is Notification ||
                playable.Route == "screen")
            {
                return true;
            }

            return IsVideoContent(playable.Content) || IsImageContent(playable.Content);
        }

        protected virtual PlayableContentQueue GetScreenContentQueue(ResourceActivationContext activationContext, PlayableContent playable)
        {
            return m_screenQueue;
        }

        protected virtual void AddToQueue(PlayableContentQueue queue, ResourceActivationContext activationContext, PlayableContent playable, PlayContentDelegate onPlay, Action onClose)
        {
            lock (m_queues)
            {
                m_queues.Add(activationContext.InstanceId, queue);
            }

            switch (playable.Priority)
            {
                case PlayableContentPriority.High:
                    // Todo: actually want to be a bit more clever and *don't* move in front of
                    // other high priority ones.
                    queue.AddToFront(
                        new QueuedPlayableContext
                        {
                            ActivationContext = activationContext,
                            Playable = playable,
                            OnClose = onClose,
                            OnPlay = onPlay
                        });

                    PlayNextSequentialContent(queue);
                    break;
                case PlayableContentPriority.Interrupt:
                    QueuedPlayableContext toStop = null;

                    lock (queue.SyncRoot)
                    {
                        toStop = queue.CurrentlyPlaying;
                    }

                    if (toStop != null)
                    {
                        StopPlaying(toStop.ActivationContext, toStop.Playable, true);
                        toStop.ActivationContext.Close();
                    }

                    queue.AddToFront(
                        new QueuedPlayableContext
                        {
                            ActivationContext = activationContext,
                            Playable = playable,
                            OnClose = onClose,
                            OnPlay = onPlay
                        });

                    PlayNextSequentialContent(queue);
                    break;
                case PlayableContentPriority.Filler:
                    // Only if nothing has played on this queue for a while...
                    bool doPlay = false;

                    lock (queue.SyncRoot)
                    {
                        doPlay = queue.CurrentlyPlaying == null &&
                            (DateTime.Now - queue.LastPlayFinish).TotalSeconds >= MinFillerDelay;
                    }

                    if (doPlay)
                    {
                        queue.Add(
                            new QueuedPlayableContext
                            {
                                ActivationContext = activationContext,
                                Playable = playable,
                                OnClose = onClose,
                                OnPlay = onPlay
                            });

                        PlayNextSequentialContent(queue);
                    }
                    break;
                case PlayableContentPriority.Normal:
                default:
                    queue.Add(
                        new QueuedPlayableContext
                        {
                            ActivationContext = activationContext,
                            Playable = playable,
                            OnClose = onClose,
                            OnPlay = onPlay
                        });

                    PlayNextSequentialContent(queue);
                    break;
            }
        }

        protected virtual void QueueScreenContent(ResourceActivationContext activationContext, PlayableContent playable, Action onClose)
        {
            var queue = GetScreenContentQueue(activationContext, playable);

            if (queue != null)
            {
                AddToQueue(queue, activationContext, playable, PlayScreenContent, onClose);
            }
            else
            {
                PlayScreenContent(activationContext, playable, onClose);
            }
        }

        void QueueAudioContent(ResourceActivationContext activationContext, PlayableContent playable, LocalizedAudioContent localizedAudio, Action onClose)
        {
            var audioRoute = GetRouteForAudioContent(playable);

            if (audioRoute == AudioContentRoute.Narrator)
            {
                AddToQueue(m_narratorQueue, activationContext, playable, PlayAudioContent, onClose);
            }
            else
            {
                PlayAudioContent(activationContext, playable, onClose);
            }
        }

        protected virtual void PlayNotification(ResourceActivationContext context, PlayableContent playable, Notification notification, ResourcePanelData<Notification> data, Action onClose)
        {
            if (Platform.Instance.IsInBackground)
            {
                context.Open();

                if (!BackgroundNotifier.Instance.HasNotification(context.InstanceId))
                {
                    var localNotification = new Motive.Core.Notifications.LocalNotification();

                    localNotification.Title = notification.Title;
                    localNotification.Text = notification.Message;

                    Platform.Instance.LocalNotificationManager.PostNotification(playable.Id, localNotification);
                }

                BackgroundNotifier.Instance.RemoveNotification(context.InstanceId);

                /*
                if (notification.SoundUrl != null)
                {
                    var path = WebServices.Instance.MediaDownloadManager.GetPathForItem(notification.SoundUrl);
                    m_channel.Play(new Uri(path));
                }*/

                if (onClose != null)
                {
                    onClose();
                }
            }
            else
            {
                if (notification.Title != null ||
                    notification.Message != null)
                {
                    Push(context, PanelStack, NotificationPanel, data, onClose);
                }
                else
                {
                    context.Open();

                    if (notification.Vibrate)
                    {
                        Platform.Instance.LocalNotificationManager.Vibrate();
                    }

                    if (notification.Sound != null)
                    {
                        Platform.Instance.PlaySound(notification.Sound.Url);
                    }

                    if (onClose != null)
                    {
                        onClose();
                    }
                }
            }
        }

        protected virtual void QueueNotification(ResourceActivationContext activationContext, PlayableContent playable, Action onClose)
        {
            var notification = (Notification)playable.Content;

            PlayNotification(activationContext, playable, notification, new ResourcePanelData<Notification>(activationContext, notification), onClose);
        }

        public override void Play(ResourceActivationContext activationContext, PlayableContent playable, Action onClose = null)
        {
            if (playable.Content is LocalizedAudioContent)
            {
                QueueAudioContent(activationContext, playable, playable.Content as LocalizedAudioContent, onClose);
            }
            else if (playable.Content is LocativeAudioContent)
            {
                m_locativeAudioProcessor.ActivateResource(activationContext, playable.Content as LocativeAudioContent);
            }
            else if (IsScreenContent(playable))
            {
                QueueScreenContent(activationContext, playable, onClose);
            }
            else
            {
                m_logger.Warning("Unhandled content type {0} (playable.Id={1})",
                    playable.Content != null ? playable.Content.Type : null,
                    playable.Id);

                if (onClose != null)
                {
                    onClose();
                }
            }
        }

        public override void StopPlaying(ResourceActivationContext ctxt, PlayableContent playable, bool interrupt = false)
        {
            PlayableContentQueue queue = null;

            lock (m_queues)
            {
                m_queues.TryGetValue(ctxt.InstanceId, out queue);
            }

            var audioContent = playable.Content as LocalizedAudioContent;

            if (audioContent != null)
            {
                if (AudioContentPlayer.Instance.StopPlaying(ctxt.InstanceId, interrupt))
                {
                    // If audio content player didn't stop in this call, the playable will
                    // stop playing and call back, we can remove it from the queue then
                    if (queue != null)
                    {
                        queue.Remove(playable);
                    }
                }
            }
            else if (playable.Content is LocativeAudioContent)
            {
                m_locativeAudioProcessor.DeactivateResource(ctxt, playable.Content as LocativeAudioContent);
            }
            else
            {
                if (queue != null)
                {
                    queue.Remove(playable);
                }
            }

            lock (m_queues)
            {
                m_queues.Remove(ctxt.InstanceId);
            }

            if (queue != null)
            {
                PlayNextSequentialContent(queue);
            }
        }

        public override void Preview(ResourceActivationContext activationContext, PlayableContent playable, DateTime playTime)
        {
            // Preview is called for playables that will be played in the future.
            // This can be useful for setting up system notifications.
        }
    }
}