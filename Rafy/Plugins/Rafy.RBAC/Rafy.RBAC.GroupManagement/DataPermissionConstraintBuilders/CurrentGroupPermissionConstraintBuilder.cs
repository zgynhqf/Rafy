/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20161220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20161220 15:41
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Rafy.Accounts;
using Rafy.Domain;
using Rafy.Domain.ORM.Query;
using Rafy.RBAC.RoleManagement;

namespace Rafy.RBAC.GroupManagement
{
    /// <summary>
    /// 组织数据权限的条件生成器
    /// </summary>
    public class CurrentGroupPermissionConstraintBuilder : DataPermissionConstraintBuilder
    {
        /// <summary>
        /// 组Id 的属性名称。
        /// </summary>
        public string GroupIdProperty
        {
            get;
            set;
        }

        /// <summary>
        /// 是否包含下级
        /// </summary>
        public bool IsIncludeChildGroup
        {
            get;
            set;
        }

        /// <summary>
        /// 构建权限过滤条件
        /// </summary>
        /// <param name="mainTable"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        protected override IConstraint BuildConstraintCore(ITableSource mainTable, IQuery query)
        {
            if (this.GroupIdProperty == null) throw new ArgumentNullException("this.GroupIdProperty");
            var currentUser = AccountContext.CurrentUser;
            var currentUserid = currentUser != null ? currentUser.Id : 0;
            var groupIdList = GetGroupListByUserId(currentUserid, IsIncludeChildGroup);
            var groupIdProperty = mainTable.EntityRepository.EntityMeta.Property(this.GroupIdProperty);
            if (groupIdProperty == null) throw new InvalidProgramException();
            return mainTable.Column(groupIdProperty.ManagedProperty).In(groupIdList);
        }

        /// <summary>
        /// 获取用户的组织列表或者及其下级组织列表
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="isIncludeChildGroup">是否包含下级</param>
        /// <returns></returns>
        protected virtual List<long> GetGroupListByUserId(long userId, bool isIncludeChildGroup)
        {
            List<long> groupIdList = new List<long>();
            var groupRepository = RepositoryFacade.ResolveInstance<GroupRepository>();
            var groupList = groupRepository.GetGroupByUserId(userId);
            if (isIncludeChildGroup)
            {
                groupIdList.AddRange(groupRepository.GetGroupAndLowerByGroupList(groupList));
            }
            else
            {
                groupIdList.AddRange(groupList.Select(p => p.Id).Cast<long>());
            }
            return groupIdList;
        }

        /// <summary>
        /// 判断Builder是否相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(DataPermissionConstraintBuilder other)
        {
            if (other == null) return false;
            CurrentGroupPermissionConstraintBuilder compareBuilder = other as CurrentGroupPermissionConstraintBuilder;
            if (compareBuilder == null) return false;
            return this.IsIncludeChildGroup.Equals(compareBuilder.IsIncludeChildGroup) && this.GroupIdProperty.Equals(compareBuilder.GroupIdProperty);
        }
    }
}