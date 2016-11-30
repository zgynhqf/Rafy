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
        public GroupRoleList GroupRoleList
        {
            get { return this.GetLazyList(GroupRoleListProperty); }
        }

        public static readonly ListProperty<GroupUserList> GroupUserListProperty = P<Group>.RegisterList(e => e.GroupUserList);
        public GroupUserList GroupUserList
        {
            get { return this.GetLazyList(GroupUserListProperty); }
        }

        #endregion

        #region 一般属性

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
        }
    }
}