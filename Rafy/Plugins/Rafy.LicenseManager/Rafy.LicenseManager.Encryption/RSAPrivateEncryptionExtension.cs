/*******************************************************
 * 
 * 作者：宋军瑞
 * 创建日期：20161017
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 宋军瑞 20161017 18:27
 * 
*******************************************************/

using System;
using System.Numerics;
using System.Security.Cryptography;

namespace Rafy.LicenseManager.Encryption
{
    /// <summary>
    /// RSA 私钥加密公钥解决扩展方法。
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class RSAPrivateEncryptionExtension
    {
        /// <summary>
        /// 私钥加密扩展方法。
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="data">表示要加密的数据。</param>
        /// <returns>返回加密后的数据。</returns>
        public static byte[] PrivateEncryption(this RSACryptoServiceProvider rsa, byte[] data)
        {
            if(data == null)
                throw new ArgumentNullException(nameof(data));
            if(rsa.PublicOnly)
                throw new InvalidOperationException("私钥未正确加载。");

            var maxDataLength = (rsa.KeySize / 8) - 6;
            if(data.Length > maxDataLength)
                throw new ArgumentOutOfRangeException(nameof(data), $"当前 key 的最大长度 ({rsa.KeySize} 位) ， 而数据长度是 {maxDataLength} 位");

            var numData = _GetBig(_AddPadding(data));

            var rsaParams = rsa.ExportParameters(true);
            var D = _GetBig(rsaParams.D);
            var modulus = _GetBig(rsaParams.Modulus);
            var encData = BigInteger.ModPow(numData, D, modulus);

            return encData.ToByteArray();
        }

        /// <summary>
        /// 公钥解密扩展方法。
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="data">表示要解密的数据。</param>
        /// <returns>返回加密前的原始数据。</returns>
        public static byte[] PublicDecryption(this RSACryptoServiceProvider rsa, byte[] data)
        {
            if(data == null)
                throw new ArgumentNullException(nameof(data));

            var numEncData = new BigInteger(data);

            var rsaParams = rsa.ExportParameters(false);
            var exponent = _GetBig(rsaParams.Exponent);
            var modulus = _GetBig(rsaParams.Modulus);

            var decData = BigInteger.ModPow(numEncData, exponent, modulus);

            var decBuffer = decData.ToByteArray();
            var result = new byte[decBuffer.Length - 1];
            Array.Copy(decBuffer, result, result.Length);
            result = _RemovePadding(result);

            Array.Reverse(result);

            return result;
        }

        private static BigInteger _GetBig(byte[] data)
        {
            var inArr = (byte[])data.Clone();
            Array.Reverse(inArr); 
            var final = new byte[inArr.Length + 1];
            Array.Copy(inArr, final, inArr.Length);

            return new BigInteger(final);
        }
        
        private static byte[] _AddPadding(byte[] data)
        {
            var rnd = new Random();
            var paddings = new byte[4];
            rnd.NextBytes(paddings);
            paddings[0] = (byte)(paddings[0] | 128);

            var results = new byte[data.Length + 4];

            Array.Copy(paddings, results, 4);
            Array.Copy(data, 0, results, 4, data.Length);

            return results;
        }

        private static byte[] _RemovePadding(byte[] data)
        {
            var results = new byte[data.Length - 4];
            Array.Copy(data, results, results.Length);

            return results;
        }
    }
}
