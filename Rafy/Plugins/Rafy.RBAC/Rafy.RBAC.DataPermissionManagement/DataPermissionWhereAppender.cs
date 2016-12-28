/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161220 14:21
 * 
*******************************************************/

using System.Collections.Generic;
using Rafy.Domain.ORM.Query;
using Rafy.RBAC.RoleManagement;

namespace Rafy.RBAC.DataPermissionManagement
{
    /// <summary>
    /// 为查询中的 Where 条件添加 数据权限过滤条件的类。
    /// </summary>
    internal class DataPermissionWhereAppender : MainTableWhereAppender
    {
        public IList<DataPermissionConstraintBuilder> ConstrainsBuilders { get; private set; } = new List<DataPermissionConstraintBuilder>();

        /// <summary>
        /// 获取过滤条件
        /// </summary>
        /// <param name="mainTable"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        protected override IConstraint GetCondition(ITableSource mainTable, IQuery query)
        {
            IConstraint res = null;
            foreach (var builder in this.ConstrainsBuilders)
            {
                var constraint = builder.BuildConstraint(mainTable, query);
                res = QueryFactory.Instance.Or(res, constraint);
            }
            return res;
        }
    }
}
