using Rafy.Domain;
using Rafy.RBAC.GroupManagement.PermissionArchitecture;
using System;

namespace Rafy.RBAC.GroupManagement.Controllers
{
    /// <summary>
    /// 权限的实例对象的控制器
    /// </summary>
    public sealed class PermissionFacadeController : DomainController, IDisposable
    {
        /// <summary>
        /// 获取权限访问对象
        /// </summary>
        /// <param name="groupID">当前登录用户所属的组的主键</param>
        /// <returns>返回权限对象实例</returns>
        public PermissionEntry Create(int groupID)
        {
            PermissionBuilder permissionBuilder = new DefaultPermissionBuilder();
            PermissionEntry permissionEntry = permissionBuilder.ResolvePermission(groupID);
            return permissionEntry;
        }

        /// <summary>
        /// 销毁对象
        /// </summary>
        public void Dispose()
        {
            return;
        }
    }
}