using Rafy.Accounts;
using Rafy.RBAC.RoleManagement;

namespace Rafy.RBAC.UserRoleManagement.Controllers
{
    /// <summary>
    /// 定义一个用户与角色的操作接口。
    /// </summary>
    public interface IUserRoleController
    {
        /// <summary>
        /// 保存一个 <see cref="UserRole"/> 到仓储。
        /// </summary>
        /// <param name="userRole">表示一个 <see cref="UserRole"/> 的实例。</param>
        void Save(UserRole userRole);

        /// <summary>
        /// 获取指定的用户 <paramref name="user"/> 是否具有指定的角色 <paramref name="role"/> 。
        /// </summary>
        /// <param name="user">表示一个用户的实例。</param>
        /// <param name="role">表示一个角色的实例。</param>
        /// <returns>true: 指定的用户 <paramref name="user"/> 有指定的角色 <paramref name="role"/> ; false: 反之。</returns>
        bool HasRole(User user, Role role);

        /// <summary>
        /// 获取指定的用户 <paramref name="user"/> 下面的角色集合 <seealso cref="RoleList"/>。
        /// </summary>
        /// <param name="user">表示一个用户的实例。</param>
        /// <returns>指定用户下的所有角色。</returns>
        RoleList GetRoleList(User user);

        /// <summary>
        /// 获取指定的角色 <paramref name="role"/> 下面的所有资源操作集合 <seealso cref="ResourceOperationList"/>。
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        ResourceOperationList GetResourceOperationList(Role role);
    }
}