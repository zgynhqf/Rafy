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
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;

namespace Rafy.RBAC.RoleManagement
{
    /// <summary>
    /// 角色功能
    /// </summary>
    [ChildEntity, Serializable]
    public class RoleOperation : RoleManagementEntity
    {
        #region 构造函数

        public RoleOperation()
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected RoleOperation(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        #endregion

        #region 引用属性
        public static readonly IRefIdProperty RoleIdProperty =
            P<RoleOperation>.RegisterRefId(e => e.RoleId, ReferenceType.Parent);
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
        public static readonly IRefIdProperty OperationIdProperty =
            P<RoleOperation>.RegisterRefId(e => e.OperationId, ReferenceType.Normal);
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
    /// 实体的领域名称 列表类。
    /// </summary>
    [Serializable]
    public partial class RoleOperationList : RoleManagementEntityList
    {
    }

    /// <summary>
    /// 实体的领域名称 仓库类。
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
        /// 获取角色的操作列表
        /// </summary>
        /// <param name="roleIds">角色集合</param>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual RoleOperationList GetByRoleIdList(List<long> roleIds)
        {
            return (RoleOperationList)this.QueryInBatches(roleIds.ToArray(), ids =>
            {
                var q = new CommonQueryCriteria();
                q.Add(RoleOperation.RoleIdProperty, PropertyOperator.In, ids);
                return this.GetBy(q);
            });
        }
        /// <summary>
        /// 获取角色的角色操作列表
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual RoleOperationList GetByRoleId(long roleId)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.RoleId == roleId);
            return (RoleOperationList)this.QueryData(q);
        }
    }

    /// <summary>
    /// 实体的领域名称 配置类。
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