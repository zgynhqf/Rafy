using System;
using Rafy.Accounts;
using Rafy.Domain;
using Rafy.RBAC.RoleManagement;

namespace Rafy.RBAC.UserRoleManagement.Controllers
{
    /// <summary>
    /// 定义一个用户与角色的操作接口。
    /// </summary>
    public interface IUserRoleController
    {
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

    /// <summary>
    /// 提供一个用户与角色的操作类。
    /// </summary>
    public class UserRoleController : DomainController, IUserRoleController
    {
        private readonly RoleRepository _roleRepository = RepositoryFacade.ResolveInstance<RoleRepository>();
        private readonly UserRoleRepository _userRoleRepository = RepositoryFacade.ResolveInstance<UserRoleRepository>();
        private readonly RoleOperationRepository _roleOperationRepository = RepositoryFacade.ResolveInstance<RoleOperationRepository>();
        private readonly ResourceOperationRepository _resourceOperationRepository = RepositoryFacade.ResolveInstance<ResourceOperationRepository>();

        /// <inheritdoc />
        public bool HasRole(User user, Role role)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            var roles = this.GetRoleList(user);
            foreach (var otherRole in roles)
            {
                if (otherRole.Id == role.Id)
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public RoleList GetRoleList(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var userRoles = _userRoleRepository.GetBy(new CommonQueryCriteria(BinaryOperator.And) {
                new PropertyMatch(UserRole.UserIdProperty, PropertyOperator.Equal, user.Id)
            });

            var results = _roleRepository.NewList();
            foreach (var userRole in userRoles)
            {
                results.Add(userRole.Role);
            }

            return results;
        }

        /// <inheritdoc />
        public ResourceOperationList GetResourceOperationList(Role role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            var roleOperations = this._roleOperationRepository.GetBy(new CommonQueryCriteria(BinaryOperator.And) {
                new PropertyMatch(RoleOperation.RoleIdProperty, PropertyOperator.Equal, role.Id)
            });
            
            if (roleOperations == null || roleOperations.Count == 0)
            {
                return this._resourceOperationRepository.NewList();
            }

            var result = this._resourceOperationRepository.NewList();
            foreach(var roleOperation in roleOperations)
            {
                result.Add(roleOperation.Operation);
            }

            return result;
        }
    }
}
