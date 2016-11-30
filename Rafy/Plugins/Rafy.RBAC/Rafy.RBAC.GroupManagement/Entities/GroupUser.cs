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
using Rafy.Accounts;

namespace Rafy.RBAC.GroupManagement
{
    /// <summary>
    /// 组和用户映射实体
    /// </summary>
    [ChildEntity, Serializable]
    public partial class GroupUser : GroupManagementEntity
    {
        #region 构造函数

        public GroupUser() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected GroupUser(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        public static readonly IRefIdProperty GroupIdProperty =
            P<GroupUser>.RegisterRefId(e => e.GroupId, ReferenceType.Parent);

        /// <summary>
        /// 组的主键
        /// </summary>
        public int GroupId
        {
            get { return (int)this.GetRefId(GroupIdProperty); }
            set { this.SetRefId(GroupIdProperty, value); }
        }
        public static readonly RefEntityProperty<Group> GroupProperty =
            P<GroupUser>.RegisterRef(e => e.Group, GroupIdProperty);

        /// <summary>
        /// 组的关联对象
        /// </summary>
        public Group Group
        {
            get { return this.GetRefEntity(GroupProperty); }
            set { this.SetRefEntity(GroupProperty, value); }
        }

        public static readonly IRefIdProperty UserIdProperty =
            P<GroupUser>.RegisterRefId(e => e.UserId, ReferenceType.Normal);

        /// <summary>
        /// 用户主键
        /// </summary>
        public long UserId
        {
            get { return (long)this.GetRefId(UserIdProperty); }
            set { this.SetRefId(UserIdProperty, value); }
        }

        public static readonly RefEntityProperty<User> UserProperty =
            P<GroupUser>.RegisterRef(e => e.User, UserIdProperty);

        /// <summary>
        /// 关联的用户实体对象
        /// </summary>
        public User User
        {
            get { return this.GetRefEntity(UserProperty); }
            set { this.SetRefEntity(UserProperty, value); }
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
    /// 组和用户映射实体 列表类。
    /// </summary>
    [Serializable]
    public partial class GroupUserList : GroupManagementEntityList { }

    /// <summary>
    /// 组和用户映射实体 仓库类。
    /// 负责 组和用户映射实体 类的查询、保存。
    /// </summary>
    public partial class GroupUserRepository : GroupManagementEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected GroupUserRepository() { }
    }

    /// <summary>
    /// 组和用户映射实体 配置类。
    /// 负责 组和用户映射实体 类的实体元数据的配置。
    /// </summary>
    internal class GroupUserConfig : GroupManagementEntityConfig<GroupUser>
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