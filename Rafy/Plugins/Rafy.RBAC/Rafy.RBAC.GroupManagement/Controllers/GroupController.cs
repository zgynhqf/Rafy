﻿/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161221
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161221 11:14
 * 
*******************************************************/

using System.Collections.Generic;
using System.Linq;
using Rafy.Domain;

namespace Rafy.RBAC.GroupManagement.Controllers
{
    /// <summary>
    /// 组织领域控制器
    /// </summary>
    public class GroupController : DomainController
    {
        /// <summary>
        /// 设置组织的用户列表
        /// 用户列表必须是当前组织的所有用户集合
        /// </summary>
        /// <param name="userIds">用户Id集合</param>
        /// <param name="groupId">组织Id</param>
        public void SetGroupUser(IList<long> userIds, long groupId)
        {
            var groupUserRepository = RepositoryFacade.ResolveInstance<GroupUserRepository>();
            var groupUserList = groupUserRepository.GetByParentId(groupId).Concrete();
            var changeGroupUserList = groupUserRepository.NewList();
            var groupUsers = groupUserList as IList<GroupUser> ?? groupUserList.ToList();
            foreach (GroupUser item in groupUsers)
            {
                if (userIds.All(id => id != item.UserId))
                {
                    changeGroupUserList.Add(item);
                    item.PersistenceStatus = PersistenceStatus.Deleted;
                }
            }
            var group = new Group { Id = groupId };
            foreach (var userId in userIds)
            {
                if (groupUsers.All(g => g.UserId != userId))
                {
                    GroupUser groupUser = new GroupUser();
                    groupUser.Group = group;
                    groupUser.User = new Accounts.User { Id = userId };
                    groupUser.PersistenceStatus = PersistenceStatus.New;
                    changeGroupUserList.Add(groupUser);
                }
            }
            groupUserRepository.Save(changeGroupUserList);
        }
    }
}
