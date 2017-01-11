﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20161220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20161220 15:43
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using Rafy.Domain.ORM.Query;

namespace Rafy.RBAC.RoleManagement
{
    /// <summary>
    /// 数据权限的条件生成器
    /// </summary>
    public abstract class DataPermissionConstraintBuilder: IEquatable<DataPermissionConstraintBuilder>
    {
        /// <summary>
        /// 构建过滤约束条件
        /// </summary>
        /// <param name="mainTable"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public IConstraint BuildConstraint(ITableSource mainTable, IQuery query)
        {
            return this.BuildConstraintCore(mainTable, query);
        }

        /// <summary>
        /// 构建过滤约束条件
        /// </summary>
        /// <param name="mainTable"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        protected abstract IConstraint BuildConstraintCore(ITableSource mainTable, IQuery query);

        /// <summary>
        /// 判断Builder是否相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract bool Equals(DataPermissionConstraintBuilder other);
    }
}