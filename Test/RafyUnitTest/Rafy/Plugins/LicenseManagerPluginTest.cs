using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy.LicenseManager.Encryption;

namespace RafyUnitTest
{
    /// <summary>
    /// 授权认证插件的单元测试类
    /// </summary>
    [TestClass]
    public class LicenseManagerPluginTest
    {
        /// <summary>
        /// 私钥
        /// </summary>
        private string _privateKey = @"<RSAKeyValue><Modulus>nr7rq0sgR0GokC/dTajW0MzTF1KJgeAhyxgMUhylsLcJVHqY4oo2SHs6uDYydfPd4m7t5uaaLmYdXTUfXDz9HNx9YwnuwDWy9GuNy7T9+ONENk/0hlfDs0bJKYgjcycu//QziY6WJi7yBZoTVSNmzj0takyoNqgSKLWhB20yTPk=</Modulus><Exponent>AQAB</Exponent><P>x5PfoPVpYh016aF28uEeV9a6FlCkqwje/ZTfQ33RnRLxb9GHfQ2SfJw1JU/CEg5Dgw0qQVkW2v1qLqbe+uBjfw==</P><Q>y5/nhdOQ7dwgqf7R8tNZFOBoi7yx9NCf776eUdLTHC7N6J2aN29ZC3y1dCp5XylSIW7N3PEE0/TVN47nVZirhw==</Q><DP>oS+a02KhZC53VmOjr/GFEihITrF+7OvTPTa5QschPh0IhejR5nvJrX5zpdjOwspmWDePwwty3BcDZP485J3JfQ==</DP><DQ>tJU+VY/4YwoqqbhEZ26J/Rq7fNm+lJgEjzDk5TnsYX0cvWQv5WPJe4eAwOH+O6fAn8fNqFjTaEokYZ5JiL7Ztw==</DQ><InverseQ>no7NPZ3OghLZx9zm5sCw0vZjfR0zmiiSrHkFgy9OMLrpi9qd9pZ1roXayQyy1ZUVCUCNO2s5mGc5v0TOlo642g==</InverseQ><D>Q055x7/rqKq7GJ9iuomqww8FNW9GZC2uxlik6K/CxMFmkE4Gwo6NY3/0LqS0EnTakCYucmc12hRrwNhEOqyVOFuDfTf7DQRRBg7GYl8XMHAecsiaLA5Tm+yOSuCC5wmOvYUN6mZ7qfWSObOwNkfpY5rCKcJlHhSgpiCJPis6k30=</D></RSAKeyValue>";

        /// <summary>
        /// 公钥
        /// </summary>
        private string _publicKey = @"<RSAKeyValue><Modulus>nr7rq0sgR0GokC/dTajW0MzTF1KJgeAhyxgMUhylsLcJVHqY4oo2SHs6uDYydfPd4m7t5uaaLmYdXTUfXDz9HNx9YwnuwDWy9GuNy7T9+ONENk/0hlfDs0bJKYgjcycu//QziY6WJi7yBZoTVSNmzj0takyoNqgSKLWhB20yTPk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        /// <summary>
        /// 校验码
        /// </summary>
        private string _checkCode = "校验码";

        /// <summary>
        /// 测试使用正确的授权码、公钥和校验码进行验证。
        /// </summary>
        [TestMethod]
        public void SA_Authenticate_Success()
        {
            DateTime expireTime = DateTime.Now.AddDays(1);

            //生成授权码
            string authenticationCode = SecurityAuthentication.Encrypt(_checkCode, expireTime, 0, _privateKey);

            //认证
            AuthorizationResult result = SecurityAuthentication.Authenticate(authenticationCode, _publicKey, _checkCode);

            Assert.IsTrue(result.Success, "未过期，结果应该是认证成功！");
        }

        /// <summary>
        /// 使用错误的校验码进行验证。
        /// </summary>
        [TestMethod]
        public void SA_SA_Authenticate_FailureByCheckCode()
        {
            DateTime expireTime = DateTime.Now.AddDays(1);

            string errorCheckCode = "错误的校验码";

            //生成授权码   
            string authenticationCode = SecurityAuthentication.Encrypt(_checkCode, expireTime, 0, _privateKey);

            //认证
            AuthorizationResult result = SecurityAuthentication.Authenticate(authenticationCode, _publicKey, errorCheckCode);

            Assert.IsTrue(!result.Success, "校验码错误，验证失败！");
            Assert.IsTrue(result.AuthorizationState == AuthorizationState.CheckCodeError, "验证失败原因：校验码错误！");
        }

        /// <summary>
        /// 使用超过授权期的时间进行验证。
        /// </summary>
        [TestMethod]
        public void SA_SA_Authenticate_FailureByExpireTime()
        {
            DateTime expireTime = new DateTime(2017, 8, 9);

            //生成授权码
            string authenticationCode = SecurityAuthentication.Encrypt(_checkCode, expireTime, 0, _privateKey);

            //认证
            AuthorizationResult result = SecurityAuthentication.Authenticate(authenticationCode, _publicKey, _checkCode);

            Assert.IsTrue(!result.Success, "当前时间超过授权期限，验证失败！");
            Assert.IsTrue(result.AuthorizationState == AuthorizationState.Expire, "验证失败原因：过期！");
        }

        /// <summary>
        /// 使用错误的授权码进行验证。
        /// </summary>
        [TestMethod]
        public void SA_SA_Authenticate_FailureByAuthCode()
        {
            DateTime expireTime = DateTime.Now.AddDays(1);

            //生成授权码
            string authenticationCode = SecurityAuthentication.Encrypt(_checkCode, expireTime, 0, _privateKey);

            string errorAuthCode = "111111111111";

            //认证
            AuthorizationResult result = SecurityAuthentication.Authenticate(errorAuthCode, _publicKey, _checkCode);

            Assert.IsTrue(!result.Success, "授权码错误，验证失败！");
        }

        /// <summary>
        /// 使用错误的公钥进行验证。
        /// </summary>
        [TestMethod]
        public void SA_SA_Authenticate_FailureByPublicKey()
        {
            DateTime expireTime = DateTime.Now.AddDays(1);

            //生成授权码
            string authenticationCode = SecurityAuthentication.Encrypt(_checkCode, expireTime, 0, _privateKey);

            string errorPublicKey = "<RSAKeyValue><Modulus>nr7rq0sgR0GokC/dTajW0MzTF1KJgeAhyxgMUhylsLcJVHqY4oo2SHs6uDYydfPd4m7t5uaaLmYdXTUfXDz9HNx9YwnuwDWy9GuNy7T9+ONENk/0hlfDs0bJKYgjcycu//QziY6WJi7yBZoTVSNmzj0takyoNqgSKLWhB20yTPk=</Modulus><Exponent>ABCD</Exponent></RSAKeyValue>";

            //认证
            AuthorizationResult result = SecurityAuthentication.Authenticate(authenticationCode, errorPublicKey, _checkCode);

            Assert.IsTrue(!result.Success, "公钥错误，验证失败！");
        }
    }
}
