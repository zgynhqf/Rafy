/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20161209
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20161209 17:58
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Accounts;
using Rafy.RBAC.RoleManagement;

namespace Rafy.RBAC.GroupManagement.PermissionArchitecture
{
    /// <summary>
    /// 权限入口类的默认实现
    /// </summary>
    public sealed class DefaultPermissionEntry : PermissionEntry
    {
        /// <summary>
        ///当前登录用户所拥有的资源权限
        /// </summary>
        public override IList<Resource> Resources {get;internal set; }

        /// <summary>
        /// 当前页面资源所对应的操作权限
        /// </summary>
        public override IDictionary<long, IList<ResourceOperation>> Operations{ get; internal set;}

        /// <summary>
        /// 当前登录用户的详情信息
        /// </summary>
        public override User User{ get; internal set;}

        /// <summary>
        /// 当前登录用户的组信息
        /// </summary>
        public override Group Group { get; internal set; }
    }
}