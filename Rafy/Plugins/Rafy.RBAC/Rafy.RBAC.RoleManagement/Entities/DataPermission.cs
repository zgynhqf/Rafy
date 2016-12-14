/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161213
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161213 17:49
 * 
*******************************************************/


using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;

namespace Rafy.RBAC.RoleManagement
{
    /// <summary>
    /// 数据权限
    /// </summary>
    [ChildEntity, Serializable]
    public partial class DataPermission : RoleManagementEntity
    {
        #region 构造函数

        public DataPermission() { }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected DataPermission(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性
        public static readonly IRefIdProperty RoleIdProperty =
           P<DataPermission>.RegisterRefId(e => e.RoleId, ReferenceType.Normal);
        public long RoleId
        {
            get { return (long)this.GetRefId(RoleIdProperty); }
            set { this.SetRefId(RoleIdProperty, value); }
        }
        public static readonly RefEntityProperty<Role> RoleProperty =
            P<DataPermission>.RegisterRef(e => e.Role, RoleIdProperty);
        /// <summary>
        /// 角色
        /// </summary>
        public Role Role
        {
            get { return this.GetRefEntity(RoleProperty); }
            set { this.SetRefEntity(RoleProperty, value); }
        }
        public static readonly IRefIdProperty ResourceIdProperty =
            P<DataPermission>.RegisterRefId(e => e.ResourceId, ReferenceType.Normal);
        public long ResourceId
        {
            get { return (long)this.GetRefId(ResourceIdProperty); }
            set { this.SetRefId(ResourceIdProperty, value); }
        }
        public static readonly RefEntityProperty<Resource> ResourceProperty =
            P<DataPermission>.RegisterRef(e => e.Resource, ResourceIdProperty);
        /// <summary>
        /// 资源
        /// </summary>
        public Resource Resource
        {
            get { return this.GetRefEntity(ResourceProperty); }
            set { this.SetRefEntity(ResourceProperty, value); }
        }
        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性
        public static readonly Property<int> ModeProperty = P<DataPermission>.Register(e => e.Mode);
        /// <summary>
        /// 数据授权模式值可自行设置 如 本人、本部门、下级部门、自定义
        /// </summary>
        public int Mode
        {
            get { return this.GetProperty(ModeProperty); }
            set { this.SetProperty(ModeProperty, value); }
        }
        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 实体的领域名称 列表类。
    /// </summary>
    [Serializable]
    public partial class DataPermissionList : RoleManagementEntityList { }

    /// <summary>
    /// 实体的领域名称 仓库类。
    /// 负责 实体的领域名称 类的查询、保存。
    /// </summary>
    public partial class DataPermissionRepository : RoleManagementEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected DataPermissionRepository() { }
    }

    /// <summary>
    /// 实体的领域名称 配置类。
    /// 负责 实体的领域名称 类的实体元数据的配置。
    /// </summary>
    internal class DataPermissionConfig : RoleManagementEntityConfig<DataPermission>
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