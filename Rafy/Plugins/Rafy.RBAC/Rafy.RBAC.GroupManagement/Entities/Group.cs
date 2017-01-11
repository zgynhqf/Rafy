/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20161212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20161212 17:03
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
using System.Data;
using Rafy.RBAC.RoleManagement;
using Rafy.RBAC.GroupManagement.Extensions;

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
        /// 查找用户所属的组织列表
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual GroupList GetGroupByUserId(long userId)
        {
            var f = QueryFactory.Instance;
            var t = f.Table<Group>();
            var t1 = f.Table<GroupUser>();
            var q = f.Query(
                selection: t.Star(),//查询所有列
                from: t.Join(t1, t.Column(Entity.IdProperty).Equal(t1.Column(GroupUser.GroupIdProperty))),//要查询的实体的表
                where: t1.Column(GroupUser.UserIdProperty).Equal(userId)
            );
            return (GroupList)this.QueryData(q);
        }

        /// <summary>
        /// 获取角色下的组织列表
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual GroupList GetGroupByRoleId(long roleId)
        {
            var f = QueryFactory.Instance;
            var t = f.Table<Group>();
            var t1 = f.Table<GroupRole>();
            var q = f.Query(
                selection: t.Star(),//查询所有列
                from: t.Join(t1, t.Column(Entity.IdProperty).Equal(t1.Column(GroupRole.GroupIdProperty))),//要查询的实体的表
                where: t1.Column(GroupRole.RoleIdProperty).Equal(roleId)
            );
            return (GroupList)this.QueryData(q);
        }

        /// <summary>
        /// 获取组织的组织Id及其子组织Id
        /// 内部是根据group的TreeIndex进行查找，所以treeIndex必须有值
        /// </summary>
        /// <param name="groupList">组织列表</param>
        /// <returns></returns>
        public List<long> GetGroupAndLowerByGroupList(GroupList groupList)
        {
            var newGroupList = new List<long>();
            groupList.EachNode(item =>
            {
                newGroupList.AddRange(
                    TreeHelper.ConvertToList<Group>(this.GetByTreeParentIndex(item.TreeIndex))
                        .Select(p => p.Id));
                return false;
            });
            return newGroupList;
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
            Meta.Property(Group.NameProperty).MapColumn().HasLength("40").IsRequired();
            Meta.Property(Group.CodeProperty).MapColumn().HasLength("100").IsRequired();
            Meta.Property(Group.DescriptionProperty).MapColumn().HasLength("200");
        }
    }
}