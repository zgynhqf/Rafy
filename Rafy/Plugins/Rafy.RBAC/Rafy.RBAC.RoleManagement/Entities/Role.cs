/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161130
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161130 10:11
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
    /// 角色
    /// </summary>
    [RootEntity, Serializable]
    public class Role : RoleManagementEntity
    {
        #region 构造函数

        public Role()
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected Role(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        #endregion

        #region 引用属性

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<Role>.Register(e => e.Name);

        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name
        {
            get { return GetProperty(NameProperty); }
            set { SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> CodeProperty = P<Role>.Register(e => e.Code);

        /// <summary>
        /// 角色编码
        /// </summary>
        public string Code
        {
            get { return GetProperty(CodeProperty); }
            set { SetProperty(CodeProperty, value); }
        }

        public static readonly Property<string> DescriptionProperty = P<Role>.Register(e => e.Description);

        /// <summary>
        /// 角色描述
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
    /// 实体的领域名称 列表类。
    /// </summary>
    [Serializable]
    public partial class RoleList : RoleManagementEntityList
    {
    }

    /// <summary>
    /// 实体的领域名称 仓库类。
    /// 负责 实体的领域名称 类的查询、保存。
    /// </summary>
    public partial class RoleRepository : RoleManagementEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected RoleRepository()
        {
        }
    }

    /// <summary>
    /// 实体的领域名称 配置类。
    /// 负责 实体的领域名称 类的实体元数据的配置。
    /// </summary>
    internal class RoleConfig : RoleManagementEntityConfig<Role>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            //配置实体的所有属性都映射到数据表中。
            Meta.MapTable().MapAllProperties();
            var namePropertyMeta = Meta.Property(Role.NameProperty).MapColumn();
            namePropertyMeta.DataTypeLength = "40";
            namePropertyMeta.IsRequired = true;
            var codePropertyMeta = Meta.Property(Role.CodeProperty).MapColumn();
            codePropertyMeta.DataTypeLength = "40";
            codePropertyMeta.IsRequired = true;
            Meta.Property(Role.DescriptionProperty).MapColumn().DataTypeLength = "200";
        }
    }
}