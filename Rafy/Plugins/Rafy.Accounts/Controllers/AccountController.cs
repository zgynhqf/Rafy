/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151209
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151209 13:06
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Rafy.Domain;

namespace Rafy.Accounts
{
    /// <summary>
    /// 帐户插件的领域控制器。
    /// </summary>
    public class AccountController : DomainController, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        public AccountController()
        {
            this.HashAlgorithm = SHA1.Create();
            this.Encoding = Encoding.UTF8;
        }

        #region Login

        /// <summary>
        /// 尝试使用指定的用户名和密码进行登录操作。
        /// 登录成功后，会重新设置 <see cref="RafyEnvironment.Principal" /> 。
        /// </summary>
        /// <param name="userName">用户名。</param>
        /// <param name="password">用户密码。</param>
        /// <param name="user">登录成功后的用户。</param>
        /// <returns></returns>
        public virtual Result Login(string userName, string password, out User user)
        {
            user = null;

            var repo = RF.Concrete<UserRepository>();
            var u = repo.GetByUserName(userName);
            if (u == null) return string.Format("没有找到用户名为：{0} 的用户。", userName);

            var passwordHashed = this.EncodePassword(password);
            if (u.Password != passwordHashed) return "密码不正确。";

            //更新用户的最后登录时间。
            u.LastLoginTime = DateTime.Now;
            repo.Save(u);

            //更新环境中的用户。
            RafyEnvironment.Principal = new GenericPrincipal(u, null);

            this.OnLoginSuccessed(u);

            user = u;

            return true;
        }

        /// <summary>
        /// 登录成功的事件。
        /// </summary>
        public event EventHandler<LoginSuccessedEventArgs> LoginSuccessed;

        /// <summary>
        /// 登录成功的事件。
        /// </summary>
        /// <param name="user"></param>
        protected virtual void OnLoginSuccessed(User user)
        {
            var handler = this.LoginSuccessed;
            if (handler != null) handler(this, new LoginSuccessedEventArgs(user));
        }

        #endregion

        #region 加密

        /// <summary>
        /// 用户密码加密使用的加密算法。
        /// 默认使用 SHA1。
        /// </summary>
        public HashAlgorithm HashAlgorithm { get; set; }

        /// <summary>
        /// 密码使用的编码方式。
        /// 默认使用 <see cref="Encoding.UTF8"/>。
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// 计算指定密码的 Hash 值。
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public virtual string EncodePassword(string password)
        {
            //创建一个计算md5的对象
            byte[] bytes = this.Encoding.GetBytes(password);
            byte[] hashBytes = this.HashAlgorithm.ComputeHash(bytes);
            var res = this.Encoding.GetString(hashBytes);
            return res;
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Finalizes an instance of the <see cref="AccountController"/> class.
        /// </summary>
        ~AccountController()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="isDisposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            var hash = this.HashAlgorithm;
            if (hash != null)
            {
                hash.Dispose();
            }
        }

        #endregion
    }

    /// <summary>
    /// 登录成功的事件参数。
    /// </summary>
    public class LoginSuccessedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginSuccessedEventArgs"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        public LoginSuccessedEventArgs(User user)
        {
            this.User = user;
        }

        /// <summary>
        /// 登录成功的用户。
        /// </summary>
        public User User { get; private set; }
    }
}