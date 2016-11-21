/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151210
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151210 18:57
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using Rafy.Domain;

namespace Rafy.Accounts
{
    /// <summary>
    /// 帐户插件的上下文数据。
    /// </summary>
    public abstract class AccountContext
    {
        /// <summary>
        /// 应用程序执行上下文中的数据项：当前用户实体。
        /// </summary>
        private static readonly AppContextItem<User> CurrentUserACI =
            new AppContextItem<User>("Rafy.Accounts.AccountHelper.CurrentUserACI");

        /// <summary>
        /// 获取当前登录的用户。如果没有登录，则此属性返回 null。
        /// <para>（获取逻辑：</para>
        /// <para>先尝试从 AppContext 中获取；</para>
        /// <para>如果没有找到，再尝试从 <see cref="RafyEnvironment.Identity"/> 中对应的身份来解析用户对象；</para>
        /// <para>如果没有找到，如果<see cref="AccountsPlugin.IsUserNameInIdentity"/> 为真，则会使用用户名来查询数据库中的用户对象。</para>
        /// <para>最终，如果找到用户，则会把用户对象存储在 AppContext 中缓存起来。方便下次使用。）</para>
        /// 开发者也可以直接设置本属性的值来指定当前的登录的用户。
        /// </summary>
        public static User CurrentUser
        {
            get
            {
                //先尝试从 AppContext 中获取，再尝试从 RafyEnvironment.Identity 中获取，最后再查询数据库。
                var user = CurrentUserACI.Value;
                if (user == null)
                {
                    var identity = RafyEnvironment.Identity;
                    user = identity as User;
                    if (user != null)
                    {
                        CurrentUserACI.Value = user;
                    }
                    else if (AccountsPlugin.IsUserNameInIdentity)
                    {
                        user = RF.ResolveInstance<UserRepository>().GetByUserName(identity.Name);
                        if (user != null)
                        {
                            CurrentUserACI.Value = user;
                        }
                    }
                }

                return user;
            }
            set
            {
                CurrentUserACI.Value = value;
            }
        }
    }
}
