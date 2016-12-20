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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Accounts;
using Rafy.Domain.ORM.Query;
using Rafy.RBAC.GroupManagement;
using Rafy.RBAC.RoleManagement;

namespace Rafy.RBAC.DataPermissionManagement
{

    /// <summary>
    /// 为查询中的 Where 条件添加 数据权限过滤条件的类。
    /// </summary>
    public class DataPermissionWhereAppender : MainTableWhereAppender
    {
        private DataPermissionFilterMode _mode;

        private Func<Group, List<long>> _getCurrentAndLowerGroup;
        public DataPermissionWhereAppender(DataPermissionFilterMode dataPermissionFilterMode, Func<Group, List<long>> getCurrentAndLowerGroup)
        {
            this._mode = dataPermissionFilterMode;
            _getCurrentAndLowerGroup = getCurrentAndLowerGroup;
        }

        /// <summary>
        /// 当前用户
        /// </summary>
        public User CurrentUser { get; set; }

        /// <summary>
        /// 当前组
        /// </summary>
        public Group CurrentGroup { get; set; }

        /// <summary>
        /// 获取过滤条件
        /// </summary>
        /// <param name="mainTable"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        protected override IConstraint GetCondition(ITableSource mainTable, IQuery query)
        {
            switch (_mode)
            {
                case DataPermissionFilterMode.CurrentUser:
                    var userColumn = mainTable.FindColumn(Resource.IdProperty);
                    return userColumn.Equal(CurrentUser);
                case DataPermissionFilterMode.CurrentOrg:
                    var orgColumn = mainTable.FindColumn(Resource.IdProperty);
                    return orgColumn.Equal(CurrentGroup.Id);
                case DataPermissionFilterMode.CurrentOrgAndLower:
                    var orgsColumn = mainTable.FindColumn(Resource.IdProperty);
                    return orgsColumn.In(_getCurrentAndLowerGroup(CurrentGroup));
                case DataPermissionFilterMode.Custom:
                    throw new Exception("不支持的过滤类型：" + _mode);
                default:
                    throw new Exception("不支持的过滤类型：" + _mode);

            }

        }
    }
}
