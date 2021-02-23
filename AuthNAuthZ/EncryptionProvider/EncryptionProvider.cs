using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AuthNAuthZ.EncryptionProvider
{
    public interface IEncryptionProvider
    {
        string Encrypt(string data, string salt = "");
        string Decrypt(string data, string salt = "");
    }
    public abstract class EncryptionProviderBase
    {
        protected string EncryptionKey { get; set; }
        public EncryptionProviderBase(string encryptionKey) { EncryptionKey = encryptionKey; }
        public void SetEncryptionKey<T>(T model, Func<T, string> getter)
        {
            EncryptionKey = getter(model);
        }
        private (byte[] Key, byte[] IV) GetKeyIV(string salt = "")
        {
            byte[][] keys = GetHashKeys($"{EncryptionKey}{salt}");
            return (keys[0], keys[1]);
        }
        protected string EncryptCore(string data, string salt = "")
        {
            string encData = null;

            try
            {
                encData = EncryptStringToBytes_Aes(data, salt);
            }
            catch (CryptographicException) { }
            catch (ArgumentNullException) { }

            return encData;
        }
        protected string DecryptCore(string data, string salt = "")
        {
            string decData = null;

            try
            {
                decData = DecryptStringFromBytes_Aes(data, salt);
            }
            catch (CryptographicException ) { }
            catch (ArgumentNullException ) { }

            return decData;
        }

        private byte[][] GetHashKeys(string key)
        {
            byte[][] result = new byte[2][];
            Encoding enc = Encoding.UTF8;

            SHA256 sha256 = new SHA256CryptoServiceProvider();

            byte[] rawKey = enc.GetBytes(key);
            byte[] rawIV = enc.GetBytes(key);

            byte[] hashKey = sha256.ComputeHash(rawKey);
            byte[] hashIV = sha256.ComputeHash(rawIV);

            Array.Resize(ref hashIV, 16);

            result[0] = hashKey;
            result[1] = hashIV;

            return result;
        }

        private string EncryptStringToBytes_Aes(string plainText, string salt = "")
        {
            var config = GetKeyIV(salt);
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (config.Key == null || config.Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (config.IV == null || config.IV.Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] encrypted;

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = config.Key;
                aesAlg.IV = config.IV;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt =
                            new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encrypted);
        }


        private string DecryptStringFromBytes_Aes(string cipherTextString, string salt = "")
        {
            var config = GetKeyIV(salt);
            byte[] cipherText = Convert.FromBase64String(cipherTextString);

            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (config.Key == null || config.Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (config.IV == null || config.IV.Length <= 0)
                throw new ArgumentNullException("IV");

            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = config.Key;
                aesAlg.IV = config.IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt =
                            new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
    }

    public sealed class EncryptionProvider : EncryptionProviderBase, IEncryptionProvider
    {
        public EncryptionProvider(string key) : base(key) { }
        public string Encrypt(string data, string salt = "") => EncryptCore(data, DateTime.UtcNow.Date.ToString() + salt);
        public string Decrypt(string data, string salt = "")
        {
            string value = DecryptCore(data, DateTime.UtcNow.Date.ToString() + salt);
            if (value == null)
            {
                return DecryptCore(data, DateTime.UtcNow.AddDays(-1).Date.ToString() + salt);
            }
            return value;
        }
    }
}
