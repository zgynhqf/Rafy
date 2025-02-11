/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161130
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161130 11:40
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Rafy.Domain;
using Rafy.Domain.ORM.Query;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;

namespace Rafy.RBAC.RoleManagement
{
    /// <summary>
    /// 角色功能
    /// </summary>
    [ChildEntity]
    public class RoleOperation : RoleManagementEntity
    {
        #region 引用属性
        public static readonly Property<int> RoleIdProperty =
            P<RoleOperation>.Register(e => e.RoleId, ReferenceType.Parent);
        public long RoleId
        {
            get { return (long)GetRefId(RoleIdProperty); }
            set { SetRefId(RoleIdProperty, value); }
        }
        public static readonly RefEntityProperty<Role> RoleProperty =
            P<RoleOperation>.RegisterRef(e => e.Role, RoleIdProperty);
        public Role Role
        {
            get { return GetRefEntity(RoleProperty); }
            set { SetRefEntity(RoleProperty, value); }
        }
        public static readonly Property<int> OperationIdProperty =
            P<RoleOperation>.Register(e => e.OperationId);
        public long OperationId
        {
            get { return (long)GetRefId(OperationIdProperty); }
            set { SetRefId(OperationIdProperty, value); }
        }
        public static readonly RefEntityProperty<ResourceOperation> OperationProperty =
            P<RoleOperation>.RegisterRef(e => e.Operation, OperationIdProperty);
        /// <summary>
        /// 资源操作
        /// </summary>
        public ResourceOperation Operation
        {
            get { return GetRefEntity(OperationProperty); }
            set { SetRefEntity(OperationProperty, value); }
        }
        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 角色功能 列表类。
    /// </summary>
    public partial class RoleOperationList : RoleManagementEntityList
    {
    }

    /// <summary>
    /// 角色功能 仓库类。
    /// 负责 实体的领域名称 类的查询、保存。
    /// </summary>
    public partial class RoleOperationRepository : RoleManagementEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected RoleOperationRepository()
        {
        }

        /// <summary>
        /// 获取角色操作的列表
        /// </summary>
        /// <param name="roleIds">角色集合</param>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual RoleOperationList GetByRoleIdList(List<long> roleIds)
        {
            var f = QueryFactory.Instance;
            var t1 = f.Table<RoleOperation>();
            var q = f.Query(
               selection: t1.Star(),//查询所有列
               from: t1,//要查询的实体的表
               where: t1.Column(RoleOperation.RoleIdProperty).In(roleIds));
            return (RoleOperationList)this.QueryData(q);
        }
    }

    /// <summary>
    /// 角色功能 配置类。
    /// 负责 实体的领域名称 类的实体元数据的配置。
    /// </summary>
    internal class RoleOperationConfig : RoleManagementEntityConfig<RoleOperation>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            //配置实体的所有属性都映射到数据表中。
            Meta.MapTable().MapAllProperties();
        }
    }
}