// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Json;
using Motive.Core.Storage;
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using Motive.Unity.Scripting;
using Motive.Unity.Storage;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Motive.Unity.Apps
{
    /// <summary>
    /// NotificationState will query ScreenMessage first for Text & ImageUrl, if ScreenMessaage is not set,
    /// then checks private _Text and _ImageUrl members
    /// </summary>
    public class NotificationState
    {
        public int ObjectId { get; set; }
        public int Sequence { get; set; }
        public bool IsRead { get; set; }

        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Text { get; set; }
        public string CharacterId { get; set; }
        public DateTime Date { get; set; }

        public CharacterMessage CharMsg;
        public ScreenMessage ScreenMsg;
    }

    public class NotificationStore : Singleton<NotificationStore>
    {

        private Dictionary<int, NotificationState> m_notificationDict;
        private List<NotificationState> m_orderedNotifications;
        private int MaxNotificationsToKeep = 12;


        public event EventHandler Updated;

        private string m_stateFile;
        private IStorageAgent m_fileAgent;

        private int NewId
        {
            get
            {
                var descendingList = m_notificationDict.Keys.ToList().OrderByDescending(obj => obj);

                var largest = descendingList.Any() ? descendingList.First() : 0;

                if (largest == int.MaxValue)
                    largest = 0;

                return largest + 1;
            }
        }

        public IEnumerable<NotificationState> AllItems
        {
            get
            {
                return m_orderedNotifications.Reverse<NotificationState>();
            }
        }

        public NotificationStore()
        {
            m_fileAgent = StorageManager.GetGameStorageAgent("notificationStore.json");

            m_notificationDict = new Dictionary<int, NotificationState>();
            m_orderedNotifications = new List<NotificationState>();

            var state = JsonHelper.Deserialize<NotificationState[]>(m_fileAgent);

            if (state != null)
            {
                foreach (var n in state)
                {
                    // This check is mostly for upgrading from
                    // an older version of the store.
                    if (n.ObjectId != 0)
                    {
                        n.Date = n.Date.ToLocalTime();
                        m_orderedNotifications.Add(n);
                        m_notificationDict[n.ObjectId] = n;
                    }
                }
            }

            ScriptManager.Instance.ScriptsReset += Instance_ScriptsReset;
        }

        void Instance_ScriptsReset(object sender, EventArgs e)
        {
            m_notificationDict.Clear();
            m_orderedNotifications.Clear();

            OnUpdated();
        }

        public void Save()
        {
            JsonHelper.Serialize(m_fileAgent, m_orderedNotifications.ToArray());
        }

        public void Remove(IEnumerable<NotificationState> notifications)
        {
            if (notifications != null)
            {
                foreach (var n in notifications)
                {
                    Remove(n, false);
                }
            }

            Save();
        }

        void Remove(NotificationState notificationState, bool commit)
        {
            if (m_notificationDict.ContainsKey(notificationState.ObjectId))
            {
                var state = m_notificationDict[notificationState.ObjectId];
                m_orderedNotifications.Remove(state);
            }

            if (commit)
            {
                Save();
            }

            OnUpdated();
        }

        public void Remove(NotificationState notificationState)
        {
            Remove(notificationState, true);
        }

        void OnUpdated()
        {
            ThreadHelper.Instance.CallOnMainThread(() =>
            {
                if (Updated != null)
                {
                    Updated(this, EventArgs.Empty);
                }
            });
        }

        private void Add(NotificationState notificationState, bool commit)
        {
            if (notificationState.ObjectId == 0)
            {
                throw new ArgumentException("Notification state must include an object ID");
            }

            RemoveExcess();

            if (!m_notificationDict.ContainsKey(notificationState.ObjectId))
            {
                m_notificationDict[notificationState.ObjectId] = notificationState;
                m_orderedNotifications.Add(notificationState);
            }

            if (commit)
            {
                Save();
            }

            OnUpdated();
        }

        private void Add(NotificationState notificationState)
        {
            Add(notificationState, true);
        }

        public void Read(int objectId)
        {
            NotificationState state;

            if (m_notificationDict.TryGetValue(objectId, out state))
            {
                state.IsRead = true;
                Save();
            }
        }

        public void MarkAllRead()
        {
            foreach (var not in m_notificationDict.Values)
            {
                not.IsRead = true;
            }

            Save();
        }

        public void Add(string text, string title, string subtitle, string imageUrl, CharacterMessage charMsg, ScreenMessage screenMsg)
        {
            Add(new NotificationState
            {
                ObjectId = NewId,
                Text = text,
                Title = title,
                Subtitle = subtitle,
                ImageUrl = imageUrl,
                Date = DateTime.Now,
                CharMsg = charMsg,
                ScreenMsg = screenMsg
            });

        }

        private void RemoveExcess()
        {
            while (m_orderedNotifications.Count > MaxNotificationsToKeep - 1) // minus 1 because about to add one
            {
                var toRemove = m_orderedNotifications.First();
                Remove(toRemove, false);
            }
        }
    }

}
