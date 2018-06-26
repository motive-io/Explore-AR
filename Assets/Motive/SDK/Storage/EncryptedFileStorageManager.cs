// Copyright (c) 2018 RocketChicken Interactive Inc.

namespace Motive.Core.Storage
{
    /// <summary>
    /// A storage manager that can be used to store data in encrypted files.
    /// </summary>
    public class EncryptedFileStorageManager : FileStorageManager
    {
        public EncryptedFileStorageManager(string rootFolder) : base(rootFolder)
        {
        }

        public override IStorageAgent GetAgent(params string[] path)
        {
            return new EncryptedFileStorageAgent(EnsureFilePath(m_rootFolder, path));
        }

        public override IStorageManager GetManager(params string[] path)
        {
            return new EncryptedFileStorageManager(EnsureFolder(m_rootFolder, path));
        }
    }
}
