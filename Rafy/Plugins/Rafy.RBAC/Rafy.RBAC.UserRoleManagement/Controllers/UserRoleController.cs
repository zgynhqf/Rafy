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
using System.Collections.Generic;
using Rafy.Accounts;
using Rafy.Domain;
using Rafy.RBAC.RoleManagement;
using System.Linq;
namespace Rafy.RBAC.UserRoleManagement.Controllers
{
    /// <summary>
    /// 提供一个用户与角色的操作类。
    /// </summary>
    public class UserRoleController : DomainController
    {
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

            return userRole != null && userRole.RoleId > 0;
        }

        /// <summary>
        /// 获取指定的用户 <paramref name="userId"/> 下面的角色集合 <seealso cref="RoleList"/>。
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns>指定用户下的所有角色。</returns>
        public virtual RoleList GetRoleList(long userId)
        {
            RoleRepository roleRepository = RepositoryFacade.ResolveInstance<RoleRepository>();
            return roleRepository.GetRoleByUserId(userId);
        }

        /// <summary>
        /// 获取指定的角色 <paramref name="role"/> 下面的用户集合 <seealso cref="UserList"/>。
        /// </summary>
        /// <param name="role">表示一个角色的实例。</param>
        /// <returns>指定角色下的所有用户的集合。</returns>
        public virtual UserList GetUserList(Role role)
        {
            UserRepository userRepository = RepositoryFacade.ResolveInstance<UserRepository>();
            return userRepository.GetUserListByRoleId(role.Id);
        }

        /// <summary>
        /// 获取指定用户、资源的操作列表
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="resourceId">资源Id</param>
        /// <returns></returns>
        public virtual ResourceOperationList GetResourceOperation(long userId, long resourceId)
        {
            var roleList = this.GetRoleList(userId);
            var roleIdList = roleList.Select(r => r.Id).Cast<long>().ToList();
            var operationIdList = RepositoryFacade.ResolveInstance<ResourceOperationRepository>().GetOperationByRoleList(roleIdList).Select(o=>(long)o.Id);
            var resourceOperationRepository = RepositoryFacade.ResolveInstance<ResourceOperationRepository>();
            var resourceOperationList = resourceOperationRepository.GetByParentId(resourceId);
            var newOperationList = resourceOperationRepository.NewList();
            var idList = operationIdList as long[] ?? operationIdList.ToArray();
            foreach (var item in resourceOperationList)
            {
                if (idList.Any(id=>id== item.Id))
                {
                    newOperationList.Add(item);
                }
            }
            return newOperationList;
        }

        /// <summary>
        /// 获取用户的资源列表
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        public virtual ResourceList GetResourceOperation(long userId)
        {
            RoleRepository roleRepository = RepositoryFacade.ResolveInstance<RoleRepository>();
            var roleList = roleRepository.GetRoleByUserId(userId);
            var roleIdList = roleList.Select(r => r.Id).Cast<long>().ToList();
            List<object> resourceIdList = new List<object>();
            var resourceOperationRepository = RepositoryFacade.ResolveInstance<ResourceOperationRepository>();
            var resourceOperationList = resourceOperationRepository.GetOperationByRoleList(roleIdList);
            foreach (var item in resourceOperationList)
            {
                if (!resourceIdList.Contains(item.ResourceId))
                {
                    resourceIdList.Add(item.ResourceId);
                }
            }
            return RepositoryFacade.ResolveInstance<ResourceRepository>().GetByIdList(resourceIdList.ToArray());
        }
    }
}
