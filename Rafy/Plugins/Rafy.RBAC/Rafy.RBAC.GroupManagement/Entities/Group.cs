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
using System.Data;
using Rafy.RBAC.RoleManagement;
using Rafy.RBAC.GroupManagement.Entities.Extensions;

namespace Rafy.RBAC.GroupManagement
{
    /// <summary>
    /// 用户组
    /// </summary>
    [RootEntity, Serializable]
    public partial class Group : GroupManagementEntity
    {
        #region 构造函数

        public Group() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Group(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        #endregion

        #region 组合子属性

        public static readonly ListProperty<GroupRoleList> GroupRoleListProperty = P<Group>.RegisterList(e => e.GroupRoleList);
        /// <summary>
        /// 可以获取组和角色关联数据列表
        /// </summary>
        public GroupRoleList GroupRoleList
        {
            get { return this.GetLazyList(GroupRoleListProperty); }
        }

        public static readonly ListProperty<GroupUserList> GroupUserListProperty = P<Group>.RegisterList(e => e.GroupUserList);
        /// <summary>
        /// 可以获取组和用户的关联数据列表
        /// </summary>
        public GroupUserList GroupUserList
        {
            get { return this.GetLazyList(GroupUserListProperty); }
        }

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<Group>.Register(e => e.Name);
        /// <summary>
        /// 组的名称
        /// </summary>
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> CodeProperty = P<Group>.Register(e => e.Code);
        /// <summary>
        /// 组编码
        /// </summary>
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<int> GroupTypeProperty = P<Group>.Register(e => e.GroupType);
        /// <summary>
        /// 组的节点的类型，0、表示组织，1、表示部门，2、表示职位
        /// </summary>
        public int GroupType
        {
            get { return this.GetProperty(GroupTypeProperty); }
            set { this.SetProperty(GroupTypeProperty, value); }
        }

        public static readonly Property<string> DescriptionProperty = P<Group>.Register(e => e.Description);
        /// <summary>
        /// 组的描述
        /// </summary>
        public string Description
        {
            get { return this.GetProperty(DescriptionProperty); }
            set { this.SetProperty(DescriptionProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 用户组 列表类。
    /// </summary>
    [Serializable]
    public partial class GroupList : GroupManagementEntityList { }

    /// <summary>
    /// 用户组 仓库类。
    /// 负责 用户组 类的查询、保存。
    /// </summary>
    public partial class GroupRepository : GroupManagementEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected GroupRepository() { }

        /// <summary>
        /// 获取当前会员组下所有资源的权限数据表
        /// </summary>
        /// <param name="groupID">用户当前使用的组的主键</param>
        /// <returns>返回获取到的当前组下的所有资源的数据过滤权限的数据表</returns>
        [RepositoryQuery]
        public virtual ResourceList GetResourcePermissionByGroupID(int groupID)
        {
            return RepositoryFacade.ResolveInstance<GroupRepository>().Extension<GroupRepositoryExtension>().GetResourcePermissionByGroupID(groupID);
        }

        /// <summary>
        /// 获取当前会员组下所有资源的权限数据表
        /// </summary>
        /// <param name="groupID">用户当前使用的组的主键</param>
        /// <returns>返回获取到的当前组下的所有资源的数据过滤权限的数据表</returns>
        [RepositoryQuery]
        public virtual ResourceOperationList GetResourceOperationPermissionByGroupID(int groupID)
        {
            return RepositoryFacade.ResolveInstance<GroupRepository>().Extension<GroupRepositoryExtension>().GetResourceOperationPermissionByGroupID(groupID);
        }
    }

    /// <summary>
    /// 用户组 配置类。
    /// 负责 用户组 类的实体元数据的配置。
    /// </summary>
    internal class GroupConfig : GroupManagementEntityConfig<Group>
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