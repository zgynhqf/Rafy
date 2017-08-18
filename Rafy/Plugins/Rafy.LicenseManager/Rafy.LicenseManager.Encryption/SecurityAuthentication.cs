/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20160921
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20160921 11:25
 * 
*******************************************************/


using System;
using System.Collections.Generic;

namespace Rafy.LicenseManager.Encryption
{
    /// <summary>
    /// 此类型是一个静态类，提供了生成授权码和验证授权码等方法。
    /// </summary>
    public static class SecurityAuthentication
    {
        private static List<string> _macList;

        /// <summary>
        /// 授权码分隔符
        /// </summary>
        private static string Flag = "┿╃";

        /// <summary>
        /// 物理网卡地址列表
        /// </summary>
        public static List<string> MacList
        {
            get { return _macList ?? (_macList = ComputerMacUtils.GetMacByNetworkInterface()); }
        }

        /// <summary>
        ///生成授权码
        /// </summary>
        /// <param name="checkCode">校验码，替换之前的物理网卡地址</param>
        /// <param name="expireTime">到时时间</param>
        /// <param name="category">授权类型0=开发 1=产品</param>
        /// <param name="privateKey">私钥</param>
        /// <returns></returns>
        public static string Encrypt(string checkCode, DateTime expireTime, int category, string privateKey)
        {
            var now = DateTime.Now.ToString("yyyyMMddHHmmss");
            var checkCodeFormart = checkCode.Replace(":", "").Replace("-", "");
            var ran = new Random();
            var randKey = ran.Next(1000, 9999);
            //14+12+10+1+4 防止同一个授权信息生成相同的授权码
            string source = $"{now}{Flag}{checkCodeFormart}{Flag}{expireTime.ToString("yyyy-MM-dd")}{Flag}{category}{Flag}{randKey}";
            return RSACryptoService.EncryptString(source, privateKey);
        }

        /// <summary>
        /// 生成授权码
        /// </summary>
        /// <param name="authorizationCode">授权信息</param>
        /// <param name="privateKey">私钥</param>
        /// <returns></returns>
        public static string Encrypt(AuthorizationCode authorizationCode, string privateKey)
        {
            return Encrypt(authorizationCode.CheckCode, authorizationCode.ExpireTime.Value, authorizationCode.Category,
                privateKey);
        }

        /// <summary>
        /// 解密授权码
        /// </summary>
        /// <param name="sSource">授权码</param>
        /// <param name="publicKey">公钥</param>
        /// <returns>授权信息</returns>
        public static AuthorizationCode Decrypt(string sSource, string publicKey)
        {
            var authorizationCode = new AuthorizationCode();
            var code = RSACryptoService.DecryptString(sSource, publicKey);

            string[] authorizationCodes = code.Split(new string[] {Flag}, StringSplitOptions.RemoveEmptyEntries);
            if (authorizationCodes.Length < 5) return authorizationCode;

            authorizationCode.CheckCode = authorizationCodes[1];
            authorizationCode.Category = Convert.ToInt16(authorizationCodes[3]);
            DateTime expireTime;
            DateTime.TryParse(authorizationCodes[2], out expireTime);
            authorizationCode.ExpireTime = expireTime;

            return authorizationCode;
        }

        /// <summary>
        /// 验证
        /// <para>此方法仅支持使用Mac地址充当校验码时的验证</para>
        /// </summary>
        /// <param name="sSource">授权码</param>
        /// <param name="publicKey">公钥</param>
        /// <returns></returns>
        public static AuthorizationResult Authenticate(string sSource, string publicKey)
        {
            var result = new AuthorizationResult();

            foreach (var mac in MacList)
            {
                //使用当前MAC码作为校验码进行验证
                result = Authenticate(sSource, publicKey, mac);

                //验证结果是通过或过期时退出循环
                if (result.Success || result.AuthorizationState == AuthorizationState.Expire) break;
            }

            return result;
        }

        /// <summary>
        /// 验证
        /// <para>此方法在验证过期时间时，与本机的当前时间进行对比</para>
        /// </summary>
        /// <param name="sSource">授权码</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="checkCode">校验码</param>
        /// <returns></returns>
        public static AuthorizationResult Authenticate(string sSource, string publicKey, string checkCode)
        {
            return Authenticate(sSource, publicKey, checkCode, DateTime.Now);
        }

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="sSource">授权码</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="checkCode">校验码</param>
        /// <param name="inspectionTime">校验日期</param>
        /// <returns></returns>
        public static AuthorizationResult Authenticate(string sSource, string publicKey, string checkCode, DateTime inspectionTime)
        {
            var authorizationCode = Decrypt(sSource, publicKey);

            var result = new AuthorizationResult
            {
                AuthorizationState = AuthorizationState.Success,
                CheckCode = authorizationCode.CheckCode,
                ExpireTime = authorizationCode.ExpireTime
            };

            if (!string.Equals(checkCode, authorizationCode.CheckCode, StringComparison.CurrentCultureIgnoreCase))
            {
                result.AuthorizationState = AuthorizationState.CheckCodeError;
                return result;
            }
            if (!authorizationCode.ExpireTime.HasValue)
            {
                result.AuthorizationState = AuthorizationState.Expire;
                return result;
            }
            if (authorizationCode.ExpireTime.Value < inspectionTime)
            {
                result.AuthorizationState = AuthorizationState.Expire;
                return result;
            }

            return result;
        }
    }
}