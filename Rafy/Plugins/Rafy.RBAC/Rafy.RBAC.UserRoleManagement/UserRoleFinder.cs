/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20161220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20161220 16:00
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Accounts;
using Rafy.Domain;
using Rafy.RBAC.RoleManagement;

namespace Rafy.RBAC.UserRoleManagement
{
    public class UserRoleFinder : IUserRoleFinder
    {
        public RoleList FindByUser(User user)
        {
            RoleRepository roleRepository = RepositoryFacade.ResolveInstance<RoleRepository>();
            UserRoleRepository userRoleRepository = RepositoryFacade.ResolveInstance<UserRoleRepository>();
            var q = new CommonQueryCriteria(BinaryOperator.And)
            {
                new PropertyMatch(UserRole.UserIdProperty, PropertyOperator.Equal, user.Id)
            };
            q.EagerLoad = new EagerLoadOptions().LoadWith(UserRole.RoleProperty);
            var userRoles = userRoleRepository.GetBy(q);
            if (userRoles == null || userRoles.Count == 0)
            {
                return roleRepository.NewList();
            }

            var results = roleRepository.NewList();
            foreach (var userRole in userRoles)
            {
                if (userRole.Role == null) continue;

                results.Add(userRole.Role);
            }
            return results;
        }
    }
}
