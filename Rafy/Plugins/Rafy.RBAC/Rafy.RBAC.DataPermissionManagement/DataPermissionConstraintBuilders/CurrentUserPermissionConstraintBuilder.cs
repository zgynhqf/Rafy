﻿/*******************************************************
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
using Rafy.Accounts;
using Rafy.Domain.ORM.Query;
using Rafy.RBAC.RoleManagement;

namespace Rafy.RBAC.DataPermissionManagement
{
    /// <summary>
    /// 用户数据权限的条件生成器
    /// </summary>
    public class CurrentUserPermissionConstraintBuilder : DataPermissionConstraintBuilder
    {
        /// <summary>
        /// 在被查询的主表中的用户 Id 的属性名称。
        /// </summary>
        public string UserIdProperty
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
            if (UserIdProperty == null) throw new ArgumentNullException("this.UserIdProperty");
            var currentUser = AccountContext.CurrentUser;
            var currentUserid = currentUser != null ? currentUser.Id : 0;//如果是匿名用户，则这个条件永远返回 False。
            var userIdProperty = mainTable.EntityRepository.EntityMeta.Property(this.UserIdProperty);
            if (userIdProperty == null) throw new InvalidProgramException();
            return mainTable.Column(userIdProperty.ManagedProperty).Equal(currentUserid);
        }

        /// <summary>
        /// 判断Builder是否相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(DataPermissionConstraintBuilder other)
        {
            if (other == null) return false;
            CurrentUserPermissionConstraintBuilder compareBuilder = other as CurrentUserPermissionConstraintBuilder;
            if (compareBuilder == null) return false;
            return this.UserIdProperty.Equals(compareBuilder.UserIdProperty);
        }
    }
}