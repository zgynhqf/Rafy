/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20161212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20161212 17:04
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.ComponentModel;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.RBAC.RoleManagement;

namespace Rafy.RBAC.GroupManagement
{
    /// <summary>
    /// 组和角色映射
    /// </summary>
    [ChildEntity, Serializable]
    public partial class GroupRole : GroupManagementEntity
    {
        #region 构造函数

        public GroupRole() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected GroupRole(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        public static readonly IRefIdProperty GroupIdProperty =
            P<GroupRole>.RegisterRefId(e => e.GroupId, ReferenceType.Parent);
        /// <summary>
        /// 组的主键
        /// </summary>
        public long GroupId
        {
            get { return (long)this.GetRefId(GroupIdProperty); }
            set { this.SetRefId(GroupIdProperty, value); }
        }

        public static readonly RefEntityProperty<Group> GroupProperty =
            P<GroupRole>.RegisterRef(e => e.Group, GroupIdProperty);
        /// <summary>
        /// 关联的组对象
        /// </summary>
        public Group Group
        {
            get { return this.GetRefEntity(GroupProperty); }
            set { this.SetRefEntity(GroupProperty, value); }
        }

        public static readonly IRefIdProperty RoleIdProperty =
            P<GroupRole>.RegisterRefId(e => e.RoleId, ReferenceType.Normal);
        /// <summary>
        /// 角色主键
        /// </summary>
        public long RoleId
        {
            get { return (long)this.GetRefId(RoleIdProperty); }
            set { this.SetRefId(RoleIdProperty, value); }
        }

        public static readonly RefEntityProperty<Role> RoleProperty =
            P<GroupRole>.RegisterRef(e => e.Role, RoleIdProperty);
        /// <summary>
        /// 关联角色实体对象
        /// </summary>
        public Role Role
        {
            get { return this.GetRefEntity(RoleProperty); }
            set { this.SetRefEntity(RoleProperty, value); }
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
    /// 组和角色映射 列表类。
    /// </summary>
    [Serializable]
    public partial class GroupRoleList : GroupManagementEntityList { }

    /// <summary>
    /// 组和角色映射 仓库类。
    /// 负责 组和角色映射 类的查询、保存。
    /// </summary>
    public partial class GroupRoleRepository : GroupManagementEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected GroupRoleRepository() { }
    }

    /// <summary>
    /// 组和角色映射 配置类。
    /// 负责 组和角色映射 类的实体元数据的配置。
    /// </summary>
    internal class GroupRoleConfig : GroupManagementEntityConfig<GroupRole>
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