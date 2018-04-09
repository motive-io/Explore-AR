// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Security.Cryptography;
using System.Text;

namespace Motive.Core.Storage
{
    /// <summary>
    /// A storage agent that stores data in encrypted files.
    /// </summary>
    public class EncryptedFileStorageAgent : FileStorageAgent
    {
        public EncryptedFileStorageAgent(string fileName) : base(fileName)
        {

        }

        public override System.IO.Stream GetReadStream()
        {
            var baseStream = base.GetReadStream();

            if (baseStream != null)
            {
                var crypto = new DESCryptoServiceProvider()
                {
                    Key = Encoding.ASCII.GetBytes(Platform.Instance.EncryptionKey), 
                    IV = Encoding.ASCII.GetBytes(Platform.Instance.EncryptionIV) 
                };

                return new CryptoStream(baseStream, crypto.CreateDecryptor(), CryptoStreamMode.Read);
            }

            return null;
        }

        public override System.IO.Stream GetWriteStream()
        {
            var crypto = new DESCryptoServiceProvider()
            {
                Key = Encoding.ASCII.GetBytes(Platform.Instance.EncryptionKey), 
                IV = Encoding.ASCII.GetBytes(Platform.Instance.EncryptionIV) 
            };

            var baseStream = base.GetWriteStream();

            return new CryptoStream(baseStream, crypto.CreateEncryptor(), CryptoStreamMode.Write);
        }
    }
}
