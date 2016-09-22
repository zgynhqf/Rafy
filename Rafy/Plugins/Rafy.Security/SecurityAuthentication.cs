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

namespace Rafy.Security
{
    public static class SecurityAuthentication
    {
        private static List<string> _macList;

        public static List<string> MacList
        {
            get { return _macList ?? (_macList = ComputerMacUtils.GetMacByNetworkInterface()); }
        }

        /// <summary>
        ///     生成授权码
        /// </summary>
        /// <param name="mac">网卡物理地址</param>
        /// <param name="expireTime">到时时间</param>
        /// <param name="category">授权类型0=开发 1=产品</param>
        /// <param name="sPublicKey">公钥</param>
        /// <returns></returns>
        public static string Encrypt(string mac, DateTime expireTime, int category, string sPublicKey)
        {
            var now = DateTime.Now.ToString("yyyyMMddHHmmss");
            var macFormart = mac.Replace(":", "").Replace("-", "");
            var ran = new Random();
            var randKey = ran.Next(1000, 9999);
            //14+12+10+1+4 防止同一个授权信息生成相同的授权码
            string source = $"{now}{macFormart}{expireTime.ToString("yyyy-MM-dd")}{category}{randKey}";
            return RSACryptoService.EncryptString(source, sPublicKey);
        }

        /// <summary>
        ///     生成授权码
        /// </summary>
        /// <param name="authorizationCode">授权信息</param>
        /// <param name="sPublicKey">公钥</param>
        /// <returns></returns>
        public static string Encrypt(AuthorizationCode authorizationCode, string sPublicKey)
        {
            return Encrypt(authorizationCode.Mac, authorizationCode.ExpireTime.Value, authorizationCode.Category,
                sPublicKey);
        }

        /// <summary>
        ///     解密授权码
        /// </summary>
        /// <param name="sSource">授权码</param>
        /// <param name="sPrivateKey">私钥</param>
        /// <returns>授权信息</returns>
        public static AuthorizationCode Decrypt(string sSource, string sPrivateKey)
        {
            var authorizationCode = new AuthorizationCode();
            var code = RSACryptoService.DecryptString(sSource, sPrivateKey);
            authorizationCode.Mac = code.Substring(14, 12);
            authorizationCode.Category = Convert.ToInt32(code.Substring(36, 1));
            DateTime expireTime;
            DateTime.TryParse(code.Substring(26, 10), out expireTime);
            authorizationCode.ExpireTime = expireTime;
            return authorizationCode;
        }

        /// <summary>
        ///     验证
        /// </summary>
        /// <param name="sSource">授权码</param>
        /// <param name="sPrivateKey">私钥</param>
        /// <returns></returns>
        public static AuthorizationResult Authenticate(string sSource, string sPrivateKey)
        {
            var result = new AuthorizationResult
            {
                AuthorizationState = AuthorizationState.Success
            };
            var authorizationCode = Decrypt(sSource, sPrivateKey);
            if (!MacList.Contains(authorizationCode.Mac.ToLower()))
            {
                result.AuthorizationState = AuthorizationState.MacError;
                return result;
            }
            if (!authorizationCode.ExpireTime.HasValue)
            {
                result.AuthorizationState = AuthorizationState.Expire;
                return result;
            }
            if (authorizationCode.ExpireTime.Value < DateTime.Now)
            {
                result.AuthorizationState = AuthorizationState.Expire;
                return result;
            }
            return result;
        }
    }
}