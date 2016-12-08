using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        bool HasRole(User user, Role role);

        RoleList GetRoleList(User user);
    }

    /// <summary>
    /// 提供一个用户与角色的操作类。
    /// </summary>
    public class UserRoleController : DomainController, IUserRoleController
    {
        private readonly RoleRepository _roleRepository = RepositoryFacade.ResolveInstance<RoleRepository>();
        private readonly UserRoleRepository _userRoleRepository = RepositoryFacade.ResolveInstance<UserRoleRepository>();

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
    }
}
