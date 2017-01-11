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
    /// <summary>
    /// 根据用户查询角色
    /// </summary>
    public class UserRoleFinder : IUserRoleFinder
    {
        /// <summary>
        /// 查询用户的所有角色
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public RoleList FindByUser(User user)
        {
            RoleRepository roleRepository = RepositoryFacade.ResolveInstance<RoleRepository>();
            return roleRepository.GetRoleByUserId(user.Id);
        }
    }
}
