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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Domain.ORM.Query;

namespace Rafy.RBAC.RoleManagement
{
    /// <summary>
    /// 数据权限的条件生成器
    /// </summary>
    public abstract class DataPermissionConstraintBuilder
    {
        /// <summary>
        /// 过滤属性字典
        /// key:value型的属性集合
        /// 如：{UserIdProperty: "UserId"}
        /// {OData:{Groupby:"Age",Orderby:"Id"}}
        /// </summary>
        public Dictionary<string, string> FilterPeoperty { get; set; }

        public IConstraint BuildConstraint(ITableSource mainTable, IQuery query)
        {
            return this.BuildConstraintCore(mainTable, query);
        }

        protected abstract IConstraint BuildConstraintCore(ITableSource mainTable, IQuery query);
    }
}