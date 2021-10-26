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

using Rafy.Domain;
using Rafy.Domain.ORM.Query;
using Rafy.RBAC.UserRoleManagement;

namespace Rafy.Accounts
{
    /// <summary>
    /// 用户仓储扩展
    /// </summary>
    public class UserRepositoryExtension : EntityRepositoryExt<UserRepository>
    {
        /// <summary>
        /// 查询角色下的用户列表
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual UserList GetUserListByRoleId(long roleId)
        {
            var f = QueryFactory.Instance;
            var t1 = f.Table<User>();
            var t2 = f.Table<UserRole>();
            var q = f.Query(
               selection: t1.Star(),//查询所有列
               from: t1.Join(t2, t1.Column(Entity.IdProperty).Equal(t2.Column(UserRole.UserIdProperty))),//要查询的实体的表
               where: t2.Column(UserRole.RoleIdProperty).Equal(roleId));
            return (UserList)this.QueryData(q);
        }
    }

    /// <summary>
    /// 用户仓储扩展
    /// </summary>
    public static class UserRepositoryExtensionHelper
    {
        /// <summary>
        /// 查询角色下的用户列表
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="roleId">角色Id</param>
        /// <returns></returns>
        public static UserList GetUserListByRoleId(this UserRepository repo, long roleId)
        {
            return repo.Extension<UserRepositoryExtension>().GetUserListByRoleId(roleId);
        }
    }
}
