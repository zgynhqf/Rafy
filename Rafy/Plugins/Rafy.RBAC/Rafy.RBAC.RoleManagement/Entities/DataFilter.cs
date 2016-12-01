/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161130
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161130 13:48
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
    ///     数据权限
    /// </summary>
    [RootEntity, Serializable]
    public class DataFilter : RoleManagementEntity
    {
        #region 构造函数

        public DataFilter()
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected DataFilter(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        #endregion

        #region 引用属性

        public static readonly IRefIdProperty ResourceIdProperty =
            P<DataFilter>.RegisterRefId(e => e.ResourceId, ReferenceType.Normal);

        public long ResourceId
        {
            get { return (long)GetRefId(ResourceIdProperty); }
            set { SetRefId(ResourceIdProperty, value); }
        }

        public static readonly RefEntityProperty<Resource> ResourceProperty =
            P<DataFilter>.RegisterRef(e => e.Resource, ResourceIdProperty);

        public static readonly IRefIdProperty RoleIdProperty =
            P<DataFilter>.RegisterRefId(e => e.RoleId, ReferenceType.Normal);

        public long RoleId
        {
            get { return (long)GetRefId(RoleIdProperty); }
            set { SetRefId(RoleIdProperty, value); }
        }

        public static readonly RefEntityProperty<Role> RoleProperty =
            P<DataFilter>.RegisterRef(e => e.Role, RoleIdProperty);

        /// <summary>
        /// </summary>
        public Role Role
        {
            get { return GetRefEntity(RoleProperty); }
            set { SetRefEntity(RoleProperty, value); }
        }

        /// <summary>
        /// </summary>
        public Resource Resource
        {
            get { return GetRefEntity(ResourceProperty); }
            set { SetRefEntity(ResourceProperty, value); }
        }

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性
        public static readonly Property<int> ModeProperty = P<DataFilter>.Register(e => e.Mode);
        /// <summary>
        /// 数据授权模式值可自行设置 如 本人、本部门、下级部门、自定义
        /// </summary>
        public int Mode
        {
            get { return this.GetProperty(ModeProperty); }
            set { this.SetProperty(ModeProperty, value); }
        }
        public static readonly Property<string> FilterRuleProperty = P<DataFilter>.Register(e => e.FilterRule);

        /// <summary>
        ///     过滤规则
        /// </summary>
        public string FilterRule
        {
            get { return GetProperty(FilterRuleProperty); }
            set { SetProperty(FilterRuleProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    ///     实体的领域名称 列表类。
    /// </summary>
    [Serializable]
    public partial class DataFilterList : RoleManagementEntityList
    {
    }

    /// <summary>
    ///     实体的领域名称 仓库类。
    ///     负责 实体的领域名称 类的查询、保存。
    /// </summary>
    public partial class DataFilterRepository : RoleManagementEntityRepository
    {
        /// <summary>
        ///     单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected DataFilterRepository()
        {

        }
    }

    /// <summary>
    ///     实体的领域名称 配置类。
    ///     负责 实体的领域名称 类的实体元数据的配置。
    /// </summary>
    internal class DataFilterConfig : RoleManagementEntityConfig<DataFilter>
    {
        /// <summary>
        ///     配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            //配置实体的所有属性都映射到数据表中。
            Meta.MapTable().MapAllProperties();
        }
    }
}