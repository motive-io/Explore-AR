// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Globalization;
using Motive.Core.Media;
using Motive.Core.Scripting;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.Models
{
    /// <summary>
    /// Motive's vision of a Unity asset bundle.
    /// </summary>
    public class AssetBundle : ScriptObject, IMediaItemProvider
    {
        public LocalizedMedia LocalizedFile { get; set; }
        public LocalizedMedia LocalizedIOSFile { get; set; }
        public LocalizedMedia LocalizedOSXFile { get; set; }
        public LocalizedMedia LocalizedAndroidFile { get; set; }
        public LocalizedMedia LocalizedWindowsFile { get; set; }

        public string Name { get; set; }
        public string Url
        {
            get
            {
                return LocalizedMedia.GetMediaUrl(File);
            }
        }

        public LocalizedMedia File
        {
            get
            {
                LocalizedMedia file = null;

                switch (UnityEngine.Application.platform)
                {
                    case RuntimePlatform.Android:
                        file = LocalizedAndroidFile;
                        break;
                    case RuntimePlatform.IPhonePlayer:
                        file = LocalizedIOSFile;
                        break;
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer:
                        file = LocalizedWindowsFile;
                        break;
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.OSXPlayer:
                        file = LocalizedOSXFile;
                        break;
                }

                if (file == null || file.MediaItem == null)
                {
                    file = LocalizedFile;
                }

                return file;
            }
        }

        public void GetMediaItems(IList<MediaItem> items)
        {
            LocalizedMedia.GetMediaItems(File, items);
        }
    }
}
