/*******************************************************
 * 
 * 作者：周金根
 * 创建时间：2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 周金根 2009
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using Rafy;
using Rafy.Domain;

namespace Rafy.RBAC.Old.Security
{
    [Serializable]
    public class RafyPrincipal : IPrincipal
    {
        #region 构造函数

        /// <summary>
        /// 由于 DataportalContext 使用了 Principel，所以每次都传输 RafyIdentity 对象，
        /// 内部的数据要尽量简单，不可以序列化此字段。
        /// </summary>
        [NonSerialized]
        private RafyIdentity _edsIdentity;

        private int _userId;

        private RafyPrincipal(RafyIdentity realIdentity)
        {
            this._userId = realIdentity.Id;
            this._edsIdentity = realIdentity;
        }

        #endregion

        #region 接口实现

        IIdentity IPrincipal.Identity
        {
            get
            {
                if (this._edsIdentity == null)
                {
                    this._edsIdentity = RF.ResolveInstance<RafyIdentityRepository>().GetById(this._userId) as RafyIdentity;
                }

                return this._edsIdentity;
            }
        }

        bool IPrincipal.IsInRole(string role)
        {
            return false;
        }

        #endregion

        /// <summary>
        /// 尝试使用指定的用户名密码登录，并返回是否成功。
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool Login(string username, string password)
        {
            var identity = RF.ResolveInstance<RafyIdentityRepository>().GetBy(username, password);
            if (identity.IsAuthenticated)
            {
                RafyEnvironment.Principal = new RafyPrincipal(identity);
            }
            else
            {
                RafyEnvironment.Principal = new AnonymousPrincipal();
            }

            return identity.IsAuthenticated;
        }

        /// <summary>
        /// 登出系统。
        /// </summary>
        public static void Logout()
        {
            RafyEnvironment.Principal = new AnonymousPrincipal();
        }
    }
}