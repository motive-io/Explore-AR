// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Storage;
using System.IO;
using UnityEngine;

/// <summary>
/// Helper that establishes the structure of the file system.
/// </summary>
namespace Motive.Unity.Storage
{
    public class StorageManager
    {
        static FileStorageManager g_gameStorageManager;
        static FileStorageManager g_appStorageManager;

        public static string GameDataFolder
        {
            get
            {
                return CombinePath(Platform.Instance.UserDataPath, "gameData");
            }
        }

        public static IStorageManager GetGameStorageManager()
        {
            if (g_gameStorageManager == null)
            {
                var folder = EnsureFolder(Platform.Instance.UserDataPath, "gameData");

                g_gameStorageManager = Platform.Instance.UseEncryption ?
                    new EncryptedFileStorageManager(folder) :
                    new FileStorageManager(folder);
            }

            return g_gameStorageManager;
        }

        public static IStorageManager GetAppStorageManager()
        {
            if (g_appStorageManager == null)
            {
                var folder = EnsureFolder(Platform.Instance.AppDataPath);

                g_appStorageManager = Platform.Instance.UseEncryption ?
                    new EncryptedFileStorageManager(folder) :
                    new FileStorageManager(folder);
            }

            return g_appStorageManager;
        }

        public static IStorageAgent GetGameStorageAgent(params string[] path)
        {
            return GetGameStorageManager().GetAgent(path);
        }

        public static IStorageAgent GetAppStorageAgent(params string[] path)
        {
            return GetAppStorageManager().GetAgent(path);
        }

        static string CombinePath(params string[] path)
        {
            string folderName = "";

            foreach (var p in path)
            {
                if (!string.IsNullOrEmpty(p))
                {
                    folderName = Path.Combine(folderName, p);
                }
            }

            return folderName;
        }

        static string EnsureFolder(string root, params string[] path)
        {
            var folder = Path.Combine(root, CombinePath(path));

            Directory.CreateDirectory(folder);

            return folder;
        }

        public static string EnsureGameFolder(params string[] path)
        {
            var folder = CombinePath(path);

            return EnsureFolder(Platform.Instance.UserDataPath, "gameData", folder);
        }

        public static string GetGameFolderName(params string[] path)
        {
            var folder = CombinePath(path);

            return CombinePath(Platform.Instance.UserDataPath, "gameData", folder);
        }

        public static bool GameFolderExists(params string[] path)
        {
            return Directory.Exists(GetGameFolderName(path));
        }

        public static string EnsureDownloadsFolder(string folder)
        {
            return EnsureFolder(Platform.Instance.AppDataPath, "downloads", folder);
        }

        public static string EnsureCatalogsFolder(string folder)
        {
            return EnsureDownloadsFolder("catalogs");
        }

        public static string EnsureCacheFolder(string folder)
        {
            return EnsureFolder(Platform.Instance.CachePath, folder);
        }

        public static string GetCatalogFileName(string spaceName, string fileName)
        {
            return GetAppFilePath("downloads", spaceName, "catalogs", fileName);
        }

        public static string GetGameFileName(string file)
        {
            return GetFilePath("gameData", file);
        }

        public static string GetGameFileName(string folder, string file)
        {
            return GetFilePath(new string[] { "gameData", folder, file });
        }

        public static string GetFilePath(params string[] filePath)
        {
            var curr = Platform.Instance.UserDataPath;

            for (int i = 0; i < filePath.Length; i++)
            {
                var p = filePath[i];

                curr = Path.Combine(curr, p);

                if (i < filePath.Length - 1)
                {
                    Directory.CreateDirectory(curr);
                }
            }

            return curr;
        }

        public static string GetAppFilePath(params string[] filePath)
        {
            var curr = Platform.Instance.AppDataPath;

            for (int i = 0; i < filePath.Length; i++)
            {
                var p = filePath[i];

                curr = Path.Combine(curr, p);

                if (i < filePath.Length - 1)
                {
                    Directory.CreateDirectory(curr);
                }
            }

            return curr;
        }

        internal static void DeleteDownloadsFolder()
        {
            var path = Path.Combine(Platform.Instance.AppDataPath, "downloads");

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        internal static void DeleteGameFolder()
        {
            var path = Path.Combine(Platform.Instance.UserDataPath, "gameData");

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        internal static void DeleteGameFolder(params string[] path)
        {
            var folderName = GetGameFolderName(path);

            if (Directory.Exists(folderName))
            {
                Directory.Delete(folderName, true);
            }
        }

        public static void RestoreGameData(params string[] path)
        {
            var fromPath = GetFilePath(path);

            if (!Directory.Exists(fromPath))
            {
                return;
            }

            var gameFolder = GameDataFolder;

            Debug.Log("***** exists " + Directory.Exists(gameFolder));

            if (Directory.Exists(gameFolder))
            {
                Directory.Delete(gameFolder, true);
            }

            Debug.Log("***** exists now " + Directory.Exists(gameFolder));

            EnsureFolder(Platform.Instance.UserDataPath, "gameData");

            //EnsureGameFolder();

            DirectoryCopy(fromPath, gameFolder, true);
        }

        public static void CopyGameData(params string[] path)
        {
            if (g_gameStorageManager != null)
            {
                g_gameStorageManager.Suspend();

                var outPath = GetFilePath(path);

                if (Directory.Exists(outPath))
                {
                    Directory.Delete(outPath, true);
                }

                DirectoryCopy(GameDataFolder, outPath, true);

                g_gameStorageManager.Resume();
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}