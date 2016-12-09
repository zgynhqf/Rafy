/*******************************************************
 * 
 * 作者：宋军瑞
 * 创建日期：20161208
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 宋军瑞 20161208 10:20
 * 
*******************************************************/

using System;
using Rafy.Accounts;
using Rafy.Domain;
using Rafy.RBAC.RoleManagement;

namespace Rafy.RBAC.UserRoleManagement.Controllers
{
    /// <summary>
    /// 提供一个用户与角色的操作类。
    /// </summary>
    public class UserRoleController : DomainController
    {
        private readonly UserRepository _userRepository = RepositoryFacade.ResolveInstance<UserRepository>();
        private readonly RoleRepository _roleRepository = RepositoryFacade.ResolveInstance<RoleRepository>();
        private readonly UserRoleRepository _userRoleRepository = RepositoryFacade.ResolveInstance<UserRoleRepository>();
        
        /// <summary>
        /// 获取指定的用户 <paramref name="user"/> 是否具有指定的角色 <paramref name="role"/> 。
        /// </summary>
        /// <param name="user">表示一个用户的实例。</param>
        /// <param name="role">表示一个角色的实例。</param>
        /// <returns>true: 指定的用户 <paramref name="user"/> 有指定的角色 <paramref name="role"/> ; false: 反之。</returns>
        public virtual bool HasRole(User user, Role role)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            var condition = new CommonQueryCriteria(BinaryOperator.And) {
                new PropertyMatch(UserRole.UserIdProperty, PropertyOperator.Equal, user.Id),
                new PropertyMatch(UserRole.RoleIdProperty, PropertyOperator.Equal, role.Id)
            };
            var userRole = this._userRoleRepository.GetFirstBy(condition);

            return userRole != null;
        }


        /// <summary>
        /// 获取指定的用户 <paramref name="user"/> 下面的角色集合 <seealso cref="RoleList"/>。
        /// </summary>
        /// <param name="user">表示一个用户的实例。</param>
        /// <returns>指定用户下的所有角色。</returns>
        public virtual RoleList GetRoleList(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var userRoles = _userRoleRepository.GetBy(new CommonQueryCriteria(BinaryOperator.And) {
                new PropertyMatch(UserRole.UserIdProperty, PropertyOperator.Equal, user.Id)
            });

            if (userRoles == null || userRoles.Count == 0)
            {
                return this._roleRepository.NewList();
            }

            var results = _roleRepository.NewList();
            foreach (var userRole in userRoles)
            {
                if(userRole.Role == null) continue;

                results.Add(userRole.Role);
            }

            return results;
        }

        /// <summary>
        /// 获取指定的角色 <paramref name="role"/> 下面的用户集合 <seealso cref="UserList"/>。
        /// </summary>
        /// <param name="role">表示一个角色的实例。</param>
        /// <returns>指定角色下的所有用户的集合。</returns>
        public virtual UserList GetUserList(Role role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            var userRoles = _userRoleRepository.GetBy(new CommonQueryCriteria(BinaryOperator.And) {
                new PropertyMatch(UserRole.RoleIdProperty, PropertyOperator.Equal, role.Id)
            });

            if (userRoles == null || userRoles.Count == 0)
            {
                return this._userRepository.NewList();
            }

            var results = this._userRepository.NewList();
            foreach(var userRole in userRoles)
            {
                if(userRole.User == null) continue;

                results.Add(userRole.User);
            }

            return results;
        }
    }
}
