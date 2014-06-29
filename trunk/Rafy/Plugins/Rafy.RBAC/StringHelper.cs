/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2010
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2010
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Rafy.RBAC
{
    /// <summary>
    /// 字符串的一些帮助方法。
    /// </summary>
    public static class CryptographyHelper
    {
        public static string MD5(string str)
        {
            return Hash(str, new MD5CryptoServiceProvider());
        }

        public static string SHA1(string str)
        {
            return Hash(str, new SHA1CryptoServiceProvider());
        }

        private static string Hash(string str, HashAlgorithm hashAlgorithm)
        {
            if (string.IsNullOrEmpty(str)) return str;

            byte[] s = hashAlgorithm.ComputeHash(UnicodeEncoding.UTF8.GetBytes(str));
            return BitConverter.ToString(s);
        }
    }
}
