/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161130
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161130 10:40
 * 
*******************************************************/


using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;

namespace Rafy.RBAC.RoleManagement
{
    /// <summary>
    /// 资源
    /// </summary>
    [RootEntity, Serializable]
    public class Resource : RoleManagementEntity
    {
        #region 构造函数

        public Resource()
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected Resource(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        #endregion

        #region 引用属性

        #endregion

        #region 组合子属性

        public static readonly ListProperty<ResourceOperationList> ResourceOperationListProperty = P<Resource>.RegisterList(e => e.ResourceOperationList);
        public ResourceOperationList ResourceOperationList
        {
            get { return this.GetLazyList(ResourceOperationListProperty); }
        }

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<Resource>.Register(e => e.Name);
        /// <summary>
        /// 资源名称
        /// </summary>
        public string Name
        {
            get { return GetProperty(NameProperty); }
            set { SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> CodeProperty = P<Resource>.Register(e => e.Code);
        /// <summary>
        /// 资源编码
        /// </summary>
        public string Code
        {
            get { return GetProperty(CodeProperty); }
            set { SetProperty(CodeProperty, value); }
        }

        public static readonly Property<string> DescriptionProperty = P<Resource>.Register(e => e.Description);
        /// <summary>
        /// 资源描述
        /// </summary>
        public string Description
        {
            get { return GetProperty(DescriptionProperty); }
            set { SetProperty(DescriptionProperty, value); }
        }

        public static readonly Property<string> ResourceEntityTypeProperty = P<Resource>.Register(e => e.ResourceEntityType);
        /// <summary>
        ///     资源领域实体类型
        /// </summary>
        public string ResourceEntityType
        {
            get { return GetProperty(ResourceEntityTypeProperty); }
            set { SetProperty(ResourceEntityTypeProperty, value); }
        }

        public static readonly Property<bool> IsSupportDataPermissionProperty = P<Resource>.Register(e => e.IsSupportDataPermission);
        /// <summary>
        /// 是否支持数据权限
        /// </summary>
        public bool IsSupportDataPermission
        {
            get { return GetProperty(IsSupportDataPermissionProperty); }
            set { SetProperty(IsSupportDataPermissionProperty, value); }
        }
        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 实体的领域名称 列表类。
    /// </summary>
    [Serializable]
    public partial class ResourceList : RoleManagementEntityList
    {
    }

    /// <summary>
    ///  实体的领域名称 仓库类。
    ///  负责 实体的领域名称 类的查询、保存。
    /// </summary>
    public partial class ResourceRepository : RoleManagementEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected ResourceRepository()
        {
        }
    }

    /// <summary>
    /// 实体的领域名称 配置类。
    /// 负责 实体的领域名称 类的实体元数据的配置。
    /// </summary>
    internal class ResourceConfig : RoleManagementEntityConfig<Resource>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            //配置实体的所有属性都映射到数据表中。
            Meta.MapTable().MapAllProperties();
            Meta.SupportTree();
        }
    }
}