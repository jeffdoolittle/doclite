using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;

namespace DocLite.Serialization
{
    public class RijndaelSerializer : ISerialize
    {
        private const int KeyLength = 16; // bytes
        private readonly byte[] _encryptionKey;
        private readonly ISerialize _inner;

        public RijndaelSerializer(ISerialize inner, byte[] encryptionKey)
        {
            if (!KeyIsValid(encryptionKey, KeyLength))
            {
                throw new ArgumentException("Invalid key length", "encryptionKey");
            }

            _encryptionKey = encryptionKey;
            _inner = inner;
        }

        public virtual void Serialize<T>(Stream output, T graph)
        {
            using (var rijndael = new RijndaelManaged())
            {
                rijndael.Key = _encryptionKey;
                rijndael.Mode = CipherMode.CBC;
                rijndael.GenerateIV();

                using (ICryptoTransform encryptor = rijndael.CreateEncryptor())
                using (var wrappedOutput = new IndisposableStream(output))
                using (var encryptionStream = new CryptoStream(wrappedOutput, encryptor, CryptoStreamMode.Write))
                {
                    wrappedOutput.Write(rijndael.IV, 0, rijndael.IV.Length);
                    _inner.Serialize(encryptionStream, graph);
                    encryptionStream.Flush();
                    encryptionStream.FlushFinalBlock();
                }
            }
        }

        public virtual T Deserialize<T>(Stream input)
        {

            using (var rijndael = new RijndaelManaged())
            {
                rijndael.Key = _encryptionKey;
                rijndael.IV = GetInitVectorFromStream(input, rijndael.IV.Length);
                rijndael.Mode = CipherMode.CBC;

                using (ICryptoTransform decryptor = rijndael.CreateDecryptor())
                using (var decryptedStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
                    return _inner.Deserialize<T>(decryptedStream);
            }
        }

        private static bool KeyIsValid(ICollection key, int length)
        {
            return key != null && key.Count == length;
        }

        private static byte[] GetInitVectorFromStream(Stream encrypted, int initVectorSizeInBytes)
        {
            var buffer = new byte[initVectorSizeInBytes];
            encrypted.Read(buffer, 0, buffer.Length);
            return buffer;
        }
    }
}