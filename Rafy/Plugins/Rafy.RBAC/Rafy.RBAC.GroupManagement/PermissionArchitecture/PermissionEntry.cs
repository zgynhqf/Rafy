using Rafy.Accounts;
using Rafy.RBAC.RoleManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.RBAC.GroupManagement.PermissionArchitecture
{
    /// <summary>
    /// 权限的入口类
    /// </summary>
    public abstract class PermissionEntry
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        protected PermissionEntry() { }

        /// <summary>
        /// 菜单模块的权限
        /// </summary>
        public abstract IList<Resource> Resources { get; internal set; }

        /// <summary>
        /// 指定模块菜单所对应的操作权限
        /// </summary>
        public abstract IDictionary<long, IList<ResourceOperation>> Operations { get; internal set; }

        /// <summary>
        /// 当前用户组的数据
        /// </summary>
        public abstract Group Group { get; internal set; }

        /// <summary>
        /// 当前登录用户的详情信息
        /// </summary>
        public abstract User User { get; internal set; }
    }
}