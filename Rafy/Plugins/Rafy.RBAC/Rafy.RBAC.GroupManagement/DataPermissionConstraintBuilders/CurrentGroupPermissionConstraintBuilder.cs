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
            get
            {
                string groupIdProperty;
                FilterPeoperty.TryGetValue("GroupIdProperty", out groupIdProperty);
                return groupIdProperty;
            }
            set { FilterPeoperty["GroupIdProperty"] = value; }
        }

        /// <summary>
        /// 是否包含下级
        /// </summary>
        public bool IsIncludeChildGroup
        {
            get
            {
                string isIncludeChildGroup;
                if (FilterPeoperty.TryGetValue("IsIncludeChildGroup", out isIncludeChildGroup))
                {
                    return Convert.ToBoolean(IsIncludeChildGroup);
                }
                return false;
            }
            set { FilterPeoperty["IsIncludeChildGroup"] = value.ToString(); }
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
            var groupIdList = GetGroupListByUserId(currentUserid);
            var groupIdProperty = mainTable.EntityRepository.EntityMeta.Property(this.GroupIdProperty);
            if (groupIdProperty == null) throw new InvalidProgramException();
            return mainTable.Column(groupIdProperty.ManagedProperty).In(groupIdList);
        }

        /// <summary>
        /// 获取用户的组织列表或者及其下级组织列表
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        protected virtual List<long> GetGroupListByUserId(long userId)
        {
            List<long> groupIdList = new List<long>();
            var groupRepository = RepositoryFacade.ResolveInstance<GroupRepository>();
            var groupList = groupRepository.GetGroupByUserId(userId);
            if (IsIncludeChildGroup)
            {
                groupIdList.AddRange(groupRepository.GetGroupAndLowerByGroupList(groupList).Select(p => p.Id).Cast<long>());
            }
            else
            {
                groupIdList.AddRange(groupList.Select(p => p.Id).Cast<long>());
            }
            return groupIdList;
        }
    }
}