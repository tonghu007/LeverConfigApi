using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using XC.RSAUtil;

namespace Lever.Common
{
    public class RsaCryptoUtils
    {
        private const int rsaKeySize = 1024;
        private const string privateKeyFileName = "private.key";
        private const string publicKeyFileName = "public.key";
        private const string keyPath = "rsa-keys";
        private static string rootPath;
        static RsaCryptoUtils()
        {
            rootPath = Directory.GetCurrentDirectory();
            string privateKeyPath = Path.Combine(rootPath, keyPath, privateKeyFileName);
            string publicKeyPath = Path.Combine(rootPath, keyPath, publicKeyFileName);
            if (!File.Exists(publicKeyPath) || !File.Exists(privateKeyPath))
                RsaCryptoUtils.GenerateKeys();
        }

        private static void GenerateKeys()
        {
            var keys = RsaKeyGenerator.Pkcs8Key(rsaKeySize, true);
            string privateKey = keys[0];
            string publicKey = keys[1];
            string path = Path.Combine(rootPath, keyPath);
            string privateKeyPath = Path.Combine(path, privateKeyFileName);
            string publicKeyPath = Path.Combine(path, publicKeyFileName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.WriteAllText(privateKeyPath, privateKey);
            File.WriteAllText(publicKeyPath, publicKey);
        }

        public static string GetPublicKey()
        {
            string publicKeyPath = Path.Combine(rootPath, keyPath, publicKeyFileName);
            return File.ReadAllText(publicKeyPath);
        }

        public static string GetPrivateKey()
        {
            string privateKeyPath = Path.Combine(rootPath, keyPath, privateKeyFileName);
            return File.ReadAllText(privateKeyPath);
        }

        public static string Encrypt(string str,string publicKey, string privateKey, int keySize)
        {
            using (var rsa = new RsaPkcs8Util(Encoding.UTF8, publicKey, privateKey, keySize))
            {
                return rsa.Encrypt(str, RSAEncryptionPadding.Pkcs1);
            }
        }

        public static string Decrypt(string str, string publicKey, string privateKey, int keySize)
        {
            using (var rsa = new RsaPkcs8Util(Encoding.UTF8, publicKey, privateKey, keySize))
            {
                return rsa.Decrypt(str, RSAEncryptionPadding.Pkcs1);
            }
        }
    }
}
