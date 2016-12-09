/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20161209
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20161209 17:50
 * 
*******************************************************/

using Rafy.Domain;
using Rafy.RBAC.GroupManagement.PermissionArchitecture;
using System;

namespace Rafy.RBAC.GroupManagement.Controllers
{
    /// <summary>
    /// 权限的实例对象的控制器
    /// </summary>
    public sealed class PermissionFacadeController : DomainController
    {
        /// <summary>
        /// 获取权限访问对象
        /// </summary>
        /// <param name="groupID">当前登录用户所属的组的主键</param>
        /// <returns>返回权限对象实例</returns>
        public PermissionEntry GetPermissionEntry(int groupID)
        {
            PermissionBuilder permissionBuilder = new DefaultPermissionBuilder();
            PermissionEntry permissionEntry = permissionBuilder.ResolvePermission(groupID);
            return permissionEntry;
        }
    }
}