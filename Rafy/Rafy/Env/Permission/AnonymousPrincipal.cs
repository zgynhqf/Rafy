/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121228 15:23
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121228 15:23
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace Rafy
{
    /// <summary>
    /// 匿名身份。
    /// </summary>
    [Serializable]
    public class AnonymousPrincipal : IPrincipal
    {
        private AnonymousIdentity _identity = new AnonymousIdentity();

        IIdentity IPrincipal.Identity
        {
            get { return this._identity; }
        }

        bool IPrincipal.IsInRole(string role)
        {
            return false;
        }
    }
}
