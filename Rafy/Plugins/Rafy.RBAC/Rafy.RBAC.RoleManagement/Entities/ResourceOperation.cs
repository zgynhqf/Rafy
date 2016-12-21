/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161213
 * 说明：此文件只包含一个类，具体内容见类型注释。$end$
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161213 16:20
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
    /// 资源操作
    /// </summary>
    [ChildEntity, Serializable]
    public class ResourceOperation : RoleManagementEntity
    {
        #region 构造函数

        public ResourceOperation()
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected ResourceOperation(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        #endregion

        #region 引用属性

        public static readonly IRefIdProperty ResourceIdProperty =
            P<ResourceOperation>.RegisterRefId(e => e.ResourceId, ReferenceType.Parent);
        public long ResourceId
        {
            get { return (long)GetRefId(ResourceIdProperty); }
            set { SetRefId(ResourceIdProperty, value); }
        }

        public static readonly RefEntityProperty<Resource> ResourceProperty =
            P<ResourceOperation>.RegisterRef(e => e.Resource, ResourceIdProperty);
        public Resource Resource
        {
            get { return GetRefEntity(ResourceProperty); }
            set { SetRefEntity(ResourceProperty, value); }
        }

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<ResourceOperation>.Register(e => e.Name);
        /// <summary>
        /// 功能名称
        /// </summary>
        public string Name
        {
            get { return GetProperty(NameProperty); }
            set { SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> CodeProperty = P<ResourceOperation>.Register(e => e.Code);
        /// <summary>
        /// 功能编码
        /// </summary>
        public string Code
        {
            get { return GetProperty(CodeProperty); }
            set { SetProperty(CodeProperty, value); }
        }


        public static readonly Property<string> DescriptionProperty = P<ResourceOperation>.Register(e => e.Description);
        /// <summary>
        /// 功能描述
        /// </summary>
        public string Description
        {
            get { return GetProperty(DescriptionProperty); }
            set { SetProperty(DescriptionProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 资源操作 列表类。
    /// </summary>
    [Serializable]
    public partial class ResourceOperationList : RoleManagementEntityList
    {
    }

    /// <summary>
    /// 资源操作 仓库类。
    /// 负责 实体的领域名称 类的查询、保存。
    /// </summary>
    public partial class ResourceOperationRepository : RoleManagementEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected ResourceOperationRepository()
        {
        }
        /// <summary>
        /// 获取资源下的资源操作列表
        /// </summary>
        /// <param name="resourceId">资源Id</param>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual ResourceOperationList GetByResourceId(long resourceId)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.ResourceId == resourceId);
            return (ResourceOperationList)this.QueryData(q);
        }

        /// <summary>
        /// 获取指定角色的操作列表
        /// </summary>
        /// <param name="roleIdList">角色Id列表</param>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual ResourceOperationList GetOperationByRoleList(List<long> roleIdList)
        {
            var f = QueryFactory.Instance;
            var t = f.Table<ResourceOperation>();
            var t1 = f.Table<RoleOperation>();
            var q = f.Query(
                selection: t.Star(),//查询所有列
                from: t.Join(t1, t.Column(Entity.IdProperty).Equal(t1.Column(RoleOperation.OperationIdProperty))),
                where: t1.Column(RoleOperation.RoleIdProperty).In(roleIdList)
            );
            return (ResourceOperationList)this.QueryData(q);
        }
    }

    /// <summary>
    /// 资源操作 配置类。
    /// 负责 实体的领域名称 类的实体元数据的配置。
    /// </summary>
    internal class ResourceOperationConfig : RoleManagementEntityConfig<ResourceOperation>
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