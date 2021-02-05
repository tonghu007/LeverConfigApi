using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Lever.Common
{
    public class StringUtils
    {
        /// <summary>
        /// 返回str的MD5码，长度为32字符；str为空时返回空字符串
        /// </summary>
        public static string MD5(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            return ByteArrayToHex(bytes);
        }

        /// <summary>
        /// 字节数组转为十六进制字符串
        /// </summary>
        public static string ByteArrayToHex(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 字节数组转为十六进制字符串
        /// </summary>
        public static byte[] HexToByteArray(string hex)
        {
            int count = hex.Length / 2;
            byte[] result = new byte[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = Byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber);
            }
            return result;
        }

        /// <summary>
        /// 取字符串的字节长度，1个汉字算两个字节
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetByteLength(string str)
        {
            if (str == null) return 0;
            int count = 0;
            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];
                count += (int)ch < 128 ? 1 : 2;
            }
            return count;
        }

        /// <summary>
        /// 按字节长度取字串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string SubstringByByte(string str, int len)
        {
            int newLen;
            return SubstringByByte(str, len, out newLen);
        }

        /// <summary>
        /// 按字节长度取字串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="len"></param>
        /// <param name="newLen"></param>
        /// <returns></returns>
        public static string SubstringByByte(string str, int len, out int newLen)
        {
            string answer = string.Empty;
            newLen = 0;
            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];
                int chLen = (int)ch < 128 ? 1 : 2;
                if (newLen + chLen > len) break;
                answer += ch;
                newLen += chLen;
            }
            return answer;
        }

        /// <summary>
        /// 按字节取字符串中部分内容
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="index">起始索引，从0开始</param>
        /// <param name="length">子串字节长度</param>
        /// <returns>子串</returns>
        public static string SubstringByByte(string str, int index, int length)
        {
            string sb = string.Empty;
            int byteIndex = 0;
            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];
                int chLen = (int)ch < 128 ? 1 : 2;

                if (byteIndex >= index && byteIndex < index + length)
                {
                    sb += ch;
                }
                byteIndex += chLen;
            }
            return sb;
        }

        /// <summary>
        /// 26字母随机取一个
        /// </summary>
        /// <returns></returns>
        public static String GetRandomChar()
        {
            Random random = new Random();
            char c = (char)(random.NextDouble() * 26 + 'a');
            return Convert.ToString(c);
        }

        /// <summary>
        /// 生成n位数的随机数
        /// </summary>
        /// <param name="n">生成随机数的位数</param>
        /// <returns></returns>
        public static int GetNumberCaptcha(int n)
        {
            int max = (int)Math.Pow(10, n);
            int min = (int)Math.Pow(10, n - 1);
            Random random = new Random();
            int x = random.Next(max - min - 1);
            x = x + min;
            return x;
        }
    }
}
