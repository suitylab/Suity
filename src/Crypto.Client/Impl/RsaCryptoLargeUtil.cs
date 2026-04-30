using System;
using System.IO;
using System.Security.Cryptography;

namespace Crypto.Client.Impl
{
    public class RsaCryptoLargeUtil : IRsaCryptoUtil
    {
        public RsaKey GenerateKeys()
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                var key = new RsaKey
                {
                    Private = rsa.ToXmlString(true),
                    Public = rsa.ToXmlString(false)
                };

                return key;
            }
        }

        public byte[] Sign(byte[] bytes, string privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                var signature = rsa.SignData(bytes, new MD5CryptoServiceProvider());
                return signature;
            }
        }

        public bool Verify(byte[] bytes, byte[] signature, string publicKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);
                return rsa.VerifyData(bytes, new MD5CryptoServiceProvider(), signature);
            }
        }

        public byte[] Encrypt(byte[] plainBytes, string publicKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);
                var bufferSize = (rsa.KeySize / 8 - 11);
                byte[] buffer = new byte[bufferSize];//block to be encrypted

                using (MemoryStream msInput = new MemoryStream(plainBytes))
                {
                    using (MemoryStream msOutput = new MemoryStream())
                    {
                        int readLen;
                        while ((readLen = msInput.Read(buffer, 0, bufferSize)) > 0)
                        {
                            byte[] dataToEnc = new byte[readLen];
                            Array.Copy(buffer, 0, dataToEnc, 0, readLen);
                            byte[] encData = rsa.Encrypt(dataToEnc, false);
                            msOutput.Write(encData, 0, encData.Length);
                        }

                        byte[] result = msOutput.ToArray();
                        rsa.Clear();
                        return result;
                    }
                }
            }
        }

        public byte[] Decrypt(byte[] encryptedBytes, string privateKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                int keySize = rsa.KeySize / 8;
                byte[] buffer = new byte[keySize];
                using (MemoryStream msInput = new MemoryStream(encryptedBytes))
                {
                    using (MemoryStream msOutput = new MemoryStream())
                    {
                        int readLen;

                        while ((readLen = msInput.Read(buffer, 0, keySize)) > 0)
                        {
                            byte[] dataToDec = new byte[readLen];
                            Array.Copy(buffer, 0, dataToDec, 0, readLen);
                            byte[] decData = rsa.Decrypt(dataToDec, false);
                            msOutput.Write(decData, 0, decData.Length);
                        }

                        byte[] result = msOutput.ToArray();
                        rsa.Clear();

                        return result;
                    }
                }
            }
        }
    }
}