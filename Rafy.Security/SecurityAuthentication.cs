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

namespace Rafy.Security
{
    internal class SecurityAuthentication
    {
        /// <summary>
        ///     生成授权码
        /// </summary>
        /// <param name="mac">网卡物理地址</param>
        /// <param name="expireTime">到时时间</param>
        /// <param name="category">授权类型0=开发 1=产品</param>
        /// <param name="sPublicKey">公钥</param>
        /// <returns></returns>
        public string Encrypt(string mac, DateTime expireTime, int category, string sPublicKey)
        {
            var now = DateTime.Now.ToString("yyyyMMddHHmmss");
            var macFormart = mac.Replace(":", "").Replace("-", "");
            var ran = new Random();
            var randKey = ran.Next(1000, 9999);
            //14+12+8+1+4 防止同一个授权码生成相同的秘钥
            string source = $"{now}{macFormart}{expireTime.ToString("yyyyMMdd")}{category}{randKey}";
            return RSACryptoService.EncryptString(source, sPublicKey);
        }

        /// <summary>
        ///     生成授权码
        /// </summary>
        /// <param name="authorizationCode">授权信息</param>
        /// <param name="sPublicKey">公钥</param>
        /// <returns></returns>
        public string Encrypt(AuthorizationCode authorizationCode, string sPublicKey)
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
        public AuthorizationCode Decrypt(string sSource, string sPrivateKey)
        {
            var authorizationCode = new AuthorizationCode();
            var code = RSACryptoService.DecryptString(sSource, sPrivateKey);
            authorizationCode.Mac = code.Substring(14, 12);
            authorizationCode.Category = Convert.ToInt32(code.Substring(34, 1));
            DateTime expireTime;
            DateTime.TryParse(code.Substring(26, 8), out expireTime);
            authorizationCode.ExpireTime = expireTime;
            return authorizationCode;
        }
        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="sSource">授权码</param>
        /// <param name="sPrivateKey">私钥</param>
        /// <returns></returns>
        public AuthorizationResult Authenticate(string sSource, string sPrivateKey)
        {
            AuthorizationResult result = new AuthorizationResult();
            AuthorizationCode authorizationCode = Decrypt(sSource, sPrivateKey);
            var macList = ComputerMacUtils.GetMacByNetworkInterface();
            if (!macList.Contains(authorizationCode.Mac))
            {
                result.AuthorizationState = AuthorizationState.MacError;
            }
            if (!authorizationCode.ExpireTime.HasValue)
            {
                result.AuthorizationState = AuthorizationState.Expire;
            }
            else
            {
                if (authorizationCode.ExpireTime.Value < DateTime.Now)
                {
                    result.AuthorizationState = AuthorizationState.Expire;
                }
            }
            return result;
        }
    }
}