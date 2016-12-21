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
using Rafy.RBAC.GroupManagement.Extensions;
namespace Rafy.RBAC.GroupManagement
{
    /// <summary>
    /// 根据组织查找角色
    /// </summary>
    public class GroupUserRoleFinder : IUserRoleFinder
    {

        /// <summary>
        /// 根据用户所在的组集合，获取组的角色列表
        /// </summary>
        /// <param name="user">用户</param>
        /// <returns></returns>
        public RoleList FindByUser(User user)
        {
           var groupList=  RepositoryFacade.ResolveInstance<GroupRepository>().GetGroupByUserId(user.Id);
            return
                RepositoryFacade.ResolveInstance<RoleRepository>()
                    .GetRoleByGroupIdList(groupList.Select(p => p.Id).Cast<long>());
        }
    }
}
