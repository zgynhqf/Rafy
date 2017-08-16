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
            var rsa = new RSACryptoServiceProvider();
            sKeys[0] = rsa.ToXmlString(true);
            sKeys[1] = rsa.ToXmlString(false);
            return sKeys;
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="sSource">加密字符</param>
        /// <param name="privateKey">公钥</param>
        /// <returns>加密后的字符</returns>
        public static string EncryptString(string sSource, string privateKey)
        {
            var enc = new UTF8Encoding();
            var bytes = enc.GetBytes(sSource);
            var crypt = new RSACryptoServiceProvider();
            crypt.FromXmlString(privateKey);
            //bytes = crypt.Encrypt(bytes, false);
            bytes = crypt.PrivateEncryption(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="sSource">解密字符</param>
        /// <param name="publicKey"> </param>
        /// <returns></returns>
        public static string DecryptString(string sSource, string publicKey)
        {
            var crypt = new RSACryptoServiceProvider();
            var enc = new UTF8Encoding();
            var bytes = Convert.FromBase64String(sSource);
            crypt.FromXmlString(publicKey);
            //var decryptbyte = crypt.Decrypt(bytes, false);
            var decryptbyte = crypt.PublicDecryption(bytes);
            return enc.GetString(decryptbyte);
        }
    }
}