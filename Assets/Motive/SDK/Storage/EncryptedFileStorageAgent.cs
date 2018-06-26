// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
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
#if !UNITY_WSA_10_0
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
#else
            throw new NotSupportedException();
#endif
        }

        public override System.IO.Stream GetWriteStream()
        {
#if !UNITY_WSA_10_0
            var crypto = new DESCryptoServiceProvider()
            {
                Key = Encoding.ASCII.GetBytes(Platform.Instance.EncryptionKey), 
                IV = Encoding.ASCII.GetBytes(Platform.Instance.EncryptionIV) 
            };

            var baseStream = base.GetWriteStream();

            return new CryptoStream(baseStream, crypto.CreateEncryptor(), CryptoStreamMode.Write);
#else
            throw new NotSupportedException();
#endif
        }
    }
}
