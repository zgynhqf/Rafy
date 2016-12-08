using System;
using Rafy.Accounts;
using Rafy.Domain;
using Rafy.RBAC.RoleManagement;

namespace Rafy.RBAC.UserRoleManagement.Controllers
{
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
        public virtual void Save(UserRole userRole)
        {
            if (userRole == null)
            {
                throw new ArgumentNullException(nameof(userRole));
            }

            this._userRoleRepository.Save(userRole);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual RoleList GetRoleList(User user)
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
                if(userRole.Role == null) continue;

                results.Add(userRole.Role);
            }

            return results;
        }

        /// <inheritdoc />
        public virtual ResourceOperationList GetResourceOperationList(Role role)
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
