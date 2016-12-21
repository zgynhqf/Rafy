/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161221
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161221 09:40
 * 
*******************************************************/


using System;
using System.Collections.Generic;
using Rafy.Domain;
using Rafy.Domain.ORM.Query;
using Rafy.RBAC.RoleManagement;
using Rafy.RBAC.UserRoleManagement;

namespace Rafy.RBAC.RoleManagement
{
    /// <summary>
    /// 角色扩展
    /// </summary>
    public class RoleRepositoryExtension : EntityRepositoryExt<RoleRepository>
    {
        /// <summary>
        /// 查询用户所有的角色集合
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual RoleList GetRoleByUserId(long userId)
        {
            var f = QueryFactory.Instance;
            var t1 = f.Table<Role>();
            var t2 = f.Table<UserRole>();
            var q = f.Query(
               selection: t1.Star(),//查询所有列
               from: t1.Join(t2, t1.Column(Entity.IdProperty).Equal(t2.Column(UserRole.RoleIdProperty))),//要查询的实体的表
               where: t2.Column(UserRole.UserIdProperty).In(userId));
            return (RoleList)this.QueryData(q);
        }
    }

    /// <summary>
    /// 角色扩展
    /// </summary>
    public static class RoleRepositoryExtensionHelper
    {

        /// <summary>
        /// 查询用户所有的角色集合
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        public static RoleList GetRoleByUserId(this RoleRepository repo, long userId)
        {
            return repo.Extension<RoleRepositoryExtension>().GetRoleByUserId(userId);
        }
    }
}
