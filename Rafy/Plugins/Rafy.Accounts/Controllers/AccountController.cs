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

namespace Rafy.Accounts.Controllers
{
    /// <summary>
    /// 帐户插件的领域控制器。
    /// </summary>
    public class AccountController : DomainController, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        protected AccountController()
        {
            this.HashAlgorithm = SHA1.Create();
            this.Encoding = Encoding.UTF8;
            this.LoginFailedTimeSpan = new TimeSpan(TimeSpan.TicksPerHour);
        }

        #region Login

        /// <summary>
        /// 一个时间间隔，表示当用户在这个时间间隔内的失败次数达到 <see cref="MaxLoginFailedTimes"/> 时，用户将不再能继续登录，直到时间过期。
        /// 默认值：一个小时。
        /// </summary>
        public TimeSpan LoginFailedTimeSpan { get; set; }

        /// <summary>
        /// 在指定时间间隔 <see cref="LoginFailedTimeSpan"/> 内，最多尝试登录失败的次数。
        /// 默认值：0，表示不需要检测。
        /// </summary>
        public int MaxLoginFailedTimes { get; set; }

        /// <summary>
        /// 尝试使用指定的用户名和密码进行登录操作。
        /// 登录成功后，会重新设置 <see cref="RafyEnvironment.Principal" /> 。
        /// </summary>
        /// <param name="userName">用户名。</param>
        /// <param name="password">用户密码。</param>
        /// <param name="user">不论登录是否成功，都返回对应的用户。（如果找不到，则返回 null。）</param>
        /// <returns></returns>
        public virtual Result Login(string userName, string password, out User user)
        {
            var repo = RF.Concrete<UserRepository>();
            user = repo.GetByUserName(userName);
            if (user == null)
            {
                return new Result(ResultCodes.UserNotExists, string.Format("登录失败，用户名或密码不正确。没有找到用户名为：{0} 的用户。", userName));
            }

            //最大登录失败次数限制。
            var maxLoginTimes = this.MaxLoginFailedTimes;
            if (maxLoginTimes > 0 && user.LoginFailedTimes >= maxLoginTimes && (DateTime.Now - user.LastLoginTime <= this.LoginFailedTimeSpan))
            {
                user.LastLoginTime = DateTime.Now;
                repo.Save(user);

                this.OnLoginFailed(user);
                return new Result(ResultCodes.LoginExceedMaxFailedTimes, "登录失败，在指定时间内，已经超过用户最大的登录次数。");
            }

            //检查用户密码。
            var passwordHashed = this.EncodePassword(password);
            if (user.Password != passwordHashed)
            {
                user.LastLoginTime = DateTime.Now;
                user.LoginFailedTimes++;
                repo.Save(user);

                this.OnLoginFailed(user);
                return new Result(ResultCodes.LoginPasswordError, "登录失败，用户名或密码不正确。");
            }

            //更新用户的最后登录时间。
            user.LastLoginTime = DateTime.Now;
            user.LoginFailedTimes = 0;

            repo.Save(user);

            //更新环境中的用户。
            RafyEnvironment.Principal = new GenericPrincipal(user, null);

            this.OnLoginSuccessed(user);

            return true;
        }

        /// <summary>
        /// 登录成功的事件。
        /// </summary>
        public event EventHandler<UserLoginEventArgs> LoginSuccessed;

        /// <summary>
        /// 登录成功的事件。
        /// </summary>
        /// <param name="user"></param>
        protected virtual void OnLoginSuccessed(User user)
        {
            var handler = this.LoginSuccessed;
            if (handler != null) handler(this, new UserLoginEventArgs(user));
        }

        /// <summary>
        /// 登录成功的事件。
        /// </summary>
        public event EventHandler<UserLoginEventArgs> LoginFailed;

        /// <summary>
        /// 登录失败的事件。
        /// </summary>
        /// <param name="user"></param>
        protected virtual void OnLoginFailed(User user)
        {
            var handler = this.LoginFailed;
            if (handler != null) handler(this, new UserLoginEventArgs(user));
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
    public class UserLoginEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserLoginEventArgs"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        public UserLoginEventArgs(User user)
        {
            this.User = user;
        }

        /// <summary>
        /// 登录成功的用户。
        /// </summary>
        public User User { get; private set; }
    }
}