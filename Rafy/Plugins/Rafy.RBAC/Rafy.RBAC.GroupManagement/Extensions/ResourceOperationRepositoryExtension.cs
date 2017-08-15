/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20161212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20161212 10:27
 * 
*******************************************************/

using Rafy.Domain;
using Rafy.Domain.ORM.Query;
using Rafy.RBAC.RoleManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.RBAC.GroupManagement.Extensions
{
    /// <summary>
    /// 资源、操作仓库扩展类
    /// </summary>
    public class ResourceOperationRepositoryExtension : EntityRepositoryExt<ResourceOperationRepository>
    {
        /// <summary>
        /// 获取当前会员组下所有资源的权限数据表
        /// </summary>
        /// <param name="groupID">用户当前使用的组的主键</param>
        /// <returns>返回获取到的当前组下的所有资源的数据过滤权限的数据表</returns>
        [RepositoryQuery]
        public virtual ResourceOperationList GetResourceOperationByGroupID(long groupID)
        {
            var f = QueryFactory.Instance;
            var groupRoleTable = f.Table<GroupRole>();
            var roleOperationTable = f.Table<RoleOperation>();
            var resourceOperationTable = f.Table<ResourceOperation>();
            var q = f.Query(
                    from: resourceOperationTable,
                    where: resourceOperationTable.Column(ResourceOperation.IdProperty).In(
                            f.Query(
                            selection: roleOperationTable.Column(RoleOperation.OperationIdProperty),
                            from: roleOperationTable,
                            where: roleOperationTable.Column(RoleOperation.RoleIdProperty).In(
                                        f.Query(
                                        selection: groupRoleTable.Column(GroupRole.RoleIdProperty),
                                        from: groupRoleTable,
                                        where: groupRoleTable.Column(GroupRole.GroupIdProperty).Equal(groupID)
                                    )
                                )
                            )
                        )
                    );
            return (ResourceOperationList)this.QueryData(q);
        }
    }

    public static class ResourceOperationRepositoryExtensionHelper
    {
        /// <summary>
        /// 获取当前会员组下所有资源的权限数据表
        /// </summary>
        /// <param name="repo">The repo.</param>
        /// <param name="groupID">用户当前使用的组的主键</param>
        /// <returns>
        /// 返回获取到的当前组下的所有资源的数据过滤权限的数据表
        /// </returns>
        public static ResourceOperationList GetResourceOperationByGroupID(this ResourceOperationRepository repo, long groupID)
        {
            return repo.Extension<ResourceOperationRepositoryExtension>().GetResourceOperationByGroupID(groupID);
        }
    }
}