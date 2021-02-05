using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Lever.Common
{
    public class AesCryptoUtils
    {
        public static string Encrypt(string val, byte[] key, byte[] iv)
        {
            if (string.IsNullOrEmpty(val))
                return null;
            using (AesManaged aes = new AesManaged())
            {
                byte[] inputByteArray = Encoding.UTF8.GetBytes(val);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        byte[] bytes = ms.ToArray();
                        return base64UrlEncode(Convert.ToBase64String(bytes));
                    }
                }
            }
        }
        public static string Decrypt(string val, byte[] key, byte[] iv)
        {
            if (string.IsNullOrEmpty(val))
                return null;
            val = base64UrlDecode(val);
            using (AesManaged aes = new AesManaged())
            {
                byte[] inputByteArray = Convert.FromBase64String(val);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(key, iv), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }

        public static string base64UrlEncode(String str)
        {
            str = str.Replace("+", "-");
            str = str.Replace("/", "_");
            return str;
        }

        public static string base64UrlDecode(String str)
        {
            str = str.Replace("-", "+");
            str = str.Replace("_", "/");
            return str;
        }
    }
}
