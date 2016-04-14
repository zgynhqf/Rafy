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
using Rafy.Domain.Validation;

namespace Rafy.Accounts.Controllers
{
    /// <summary>
    /// 帐户插件的领域控制器。
    /// </summary>
    public class AccountController : DomainController, IDisposable
    {
        private UserIdentityMode _identityMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        protected AccountController()
        {
            this.HashAlgorithm = SHA1.Create();
            this.Encoding = Encoding.UTF8;
            this.LoginFailedTimeSpan = new TimeSpan(TimeSpan.TicksPerHour);
            _identityMode = UserIdentityMode.UserName;
        }

        /// <summary>
        /// 是否使用用户名为登录标识。默认 <see cref="UserIdentityMode.UserName"/>。
        /// </summary>
        public UserIdentityMode IdentityMode
        {
            get { return _identityMode; }
            set { _identityMode = value; }
        }

        #region Register

        /// <summary>
        /// 注册指定的用户。
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual Result Register(User user)
        {
            if (user == null) throw new ArgumentNullException("user");
            var userNameAsId = _identityMode.HasFlag(UserIdentityMode.UserName);
            if (userNameAsId && string.IsNullOrEmpty(user.UserName)) return new Result(ResultCodes.RegisterUserNameInvalid, "用户名不能为空。");
            var emailAsId = _identityMode.HasFlag(UserIdentityMode.Email);
            if (emailAsId && !TextFormatter.ReEmail.IsMatch(user.Email)) return new Result(ResultCodes.RegisterEmailInvalid, "邮箱格式不正确。");
            if (!userNameAsId && !emailAsId) throw new InvalidProgramException("!userNameAsId && !useEmailAsId");

            //验证其它属性。
            var brokenRules = Validator.Validate(user);
            if (brokenRules.Count > 0) return new Result(ResultCodes.RegisterPropertiesInvalid, brokenRules.ToString());

            //检查用户名、邮箱的重复性。
            var repo = RF.Concrete<UserRepository>();
            var criteria = new CommonQueryCriteria();
            criteria.Concat = BinaryOperator.Or;
            if (userNameAsId)
            {
                criteria.Add(new PropertyMatch(User.UserNameProperty, user.UserName));
            }
            if (emailAsId)
            {
                criteria.Add(new PropertyMatch(User.EmailProperty, user.Email));
            }
            var exists = repo.GetFirstBy(criteria);
            if (exists != null)
            {
                if (emailAsId && exists.Email == user.Email)
                {
                    return new Result(ResultCodes.RegisterEmailDuplicated, string.Format("注册失败，已经存在邮箱为：{0} 的用户。", user.Email));
                }
                else
                {
                    return new Result(ResultCodes.RegisterUserNameDuplicated, string.Format("注册失败，已经存在用户名为：{0} 的用户。", user.UserName));
                }
            }

            //保存这个用户
            user.PersistenceStatus = PersistenceStatus.New;
            repo.Save(user);

            this.OnRegisterSuccessed(user);

            return true;
        }

        /// <summary>
        /// 注册成功的事件。
        /// </summary>
        public event EventHandler<AccountEventArgs> RegisterSuccessed;

        /// <summary>
        /// 注册成功的事件。
        /// </summary>
        /// <param name="user"></param>
        protected virtual void OnRegisterSuccessed(User user)
        {
            var handler = this.RegisterSuccessed;
            if (handler != null) handler(this, new AccountEventArgs(user));
        }

        #endregion

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
        public virtual Result LoginByUserName(string userName, string password, out User user)
        {
            var repo = RF.Concrete<UserRepository>();
            user = repo.GetByUserName(userName);
            if (user == null)
            {
                return new Result(ResultCodes.LoginUserNotExists, string.Format("登录失败，用户名或密码不正确。没有找到用户名为：{0} 的用户。", userName));
            }

            return this.LoginCore(user, password);
        }

        /// <summary>
        /// 尝试使用指定的邮箱和密码进行登录操作。
        /// 登录成功后，会重新设置 <see cref="RafyEnvironment.Principal" /> 。
        /// </summary>
        /// <param name="email">用户名。</param>
        /// <param name="password">用户密码。</param>
        /// <param name="user">不论登录是否成功，都返回对应的用户。（如果找不到，则返回 null。）</param>
        /// <returns></returns>
        public virtual Result LoginByEmail(string email, string password, out User user)
        {
            var repo = RF.Concrete<UserRepository>();
            user = repo.GetByEmail(email);
            if (user == null)
            {
                return new Result(ResultCodes.LoginUserNotExists, string.Format("登录失败，邮箱或密码不正确。没有找到用户名为：{0} 的用户。", email));
            }

            return this.LoginCore(user, password);
        }

        /// <summary>
        /// 用指定的密码来尝试登录指定的用户。（登录的核心逻辑。）
        /// 登录成功后，会重新设置 <see cref="RafyEnvironment.Principal" /> 。
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        protected virtual Result LoginCore(User user, string password)
        {
            var repo = RF.Concrete<UserRepository>();

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
        public event EventHandler<AccountEventArgs> LoginSuccessed;

        /// <summary>
        /// 登录成功的事件。
        /// </summary>
        /// <param name="user"></param>
        protected virtual void OnLoginSuccessed(User user)
        {
            var handler = this.LoginSuccessed;
            if (handler != null) handler(this, new AccountEventArgs(user));
        }

        /// <summary>
        /// 登录成功的事件。
        /// </summary>
        public event EventHandler<AccountEventArgs> LoginFailed;

        /// <summary>
        /// 登录失败的事件。
        /// </summary>
        /// <param name="user"></param>
        protected virtual void OnLoginFailed(User user)
        {
            var handler = this.LoginFailed;
            if (handler != null) handler(this, new AccountEventArgs(user));
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
    public class AccountEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountEventArgs"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        public AccountEventArgs(User user)
        {
            this.User = user;
        }

        /// <summary>
        /// 对应的用户。
        /// </summary>
        public User User { get; private set; }
    }

    /// <summary>
    /// 用户标识模式
    /// </summary>
    [Flags]
    public enum UserIdentityMode
    {
        /// <summary>
        /// 用户名必填， 且是唯一标识。
        /// </summary>
        UserName = 1,
        /// <summary>
        /// 邮箱地址必填， 且是唯一标识。
        /// </summary>
        Email = 2
    }
}