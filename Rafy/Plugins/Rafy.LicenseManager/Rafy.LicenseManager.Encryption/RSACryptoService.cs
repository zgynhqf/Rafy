/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20160921
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20160921 09:36
 * 修改文件 崔化栋 20180528 18:00
 *   增加NS2的支持；
 * 
*******************************************************/


using System;
using System.Security.Cryptography;
using System.Text;

namespace Rafy.LicenseManager.Encryption
{
    /// <summary>
    /// RSA加密
    /// </summary>
    public static class RSACryptoService
    {
        /// <summary>
        /// 生成公钥私钥
        /// </summary>
        /// <returns>公钥=arr[1] 私钥=arr[0]</returns>
        public static string[] GenerateKeys()
        {
            var sKeys = new string[2];
#if NET45
            var rsa = new RSACryptoServiceProvider();
            sKeys[0] = rsa.ToXmlString(true);
            sKeys[1] = rsa.ToXmlString(false);
#endif
#if NETSTANDARD2_0 || NETCOREAPP2_0
            var rsa = RSA.Create();
            sKeys[0] = rsa.ToXMLString(true);
            sKeys[1] = rsa.ToXMLString(false);
#endif
            return sKeys;
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="sSource">加密字符</param>
        /// <param name="publicKey">公钥</param>
        /// <returns>加密后的字符</returns>
        public static string EncryptString(string sSource, string publicKey)
        {
            var enc = new UTF8Encoding();
            var bytes = enc.GetBytes(sSource);
#if NET45
            var crypt = new RSACryptoServiceProvider();
            crypt.FromXmlString(publicKey);
            bytes = crypt.PrivateEncryption(bytes);
#endif
#if NETSTANDARD2_0 || NETCOREAPP2_0
            var crypt = RSA.Create();
            crypt.FromXMLString(publicKey);
            bytes = crypt.Encrypt(bytes, RSAEncryptionPadding.Pkcs1);
#endif
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="sSource">解密字符</param>
        /// <param name="privateKey"> </param>
        /// <returns></returns>
        public static string DecryptString(string sSource, string privateKey)
        {
            var enc = new UTF8Encoding();
            var bytes = Convert.FromBase64String(sSource);
#if NET45
            var crypt = new RSACryptoServiceProvider();
            crypt.FromXmlString(privateKey);
            var decryptbyte = crypt.PublicDecryption(bytes);
#endif
#if NETSTANDARD2_0 || NETCOREAPP2_0
            var crypt = RSA.Create();
            crypt.FromXMLString(privateKey);
            var decryptbyte = crypt.Decrypt(bytes, RSAEncryptionPadding.Pkcs1);
#endif
            return enc.GetString(decryptbyte);
        }
    }
}