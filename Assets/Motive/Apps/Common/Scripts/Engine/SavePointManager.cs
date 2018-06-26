// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Json;
using Motive.Core.Scripting;
using Motive.Core.Storage;
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using Motive.Unity.Scripting;
using Motive.Unity.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Apps
{
    /// <summary>
    /// Manages a set of "save points". A save point captures the
    /// app state at a given time and allows the user to restart
    /// from that point.
    /// </summary>
    public class SavePointManager : Singleton<SavePointManager>
    {
        string m_filePath;

        public class SavePointState
        {
            public DateTime Time { get; set; }
            public SavePoint Checkpoint { get; set; }
        }

        public IEnumerable<SavePointState> Checkpoints
        {
            get
            {
                return m_checkpoints.ToArray();
            }
        }

        List<SavePointState> m_checkpoints;

        public SavePointManager()
        {
            m_filePath = StorageManager.GetFilePath("savePoints", "savePoints.meta");

            var checkpoints = JsonHelper.Deserialize<SavePointState[]>(m_filePath);

            if (checkpoints == null)
            {
                m_checkpoints = new List<SavePointState>();
            }
            else
            {
                m_checkpoints = checkpoints.ToList();
            }
        }

        public void ActivateSavePoint(ResourceActivationContext context, SavePoint resource)
        {
            StorageManager.CopyGameData("savePoints", resource.Id);

            m_checkpoints.RemoveAll(c => c.Checkpoint.Id == resource.Id);

            m_checkpoints.Add(new SavePointState
            {
                Time = DateTime.Now,
                Checkpoint = resource
            });

            SaveState();
        }

        void SaveState()
        {
            JsonHelper.Serialize(m_filePath, m_checkpoints);
        }

        /// <summary>
        /// Restores the app from the given save point.
        /// </summary>
        /// <param name="checkpoint"></param>
        public void Restore(SavePoint checkpoint)
        {
            ScriptManager.Instance.StopAllProcessors(() =>
                {
                    StorageManager.RestoreGameData("savePoints", checkpoint.Id);

                    ScriptManager.Instance.RunScripts();
                });
        }
    }
}