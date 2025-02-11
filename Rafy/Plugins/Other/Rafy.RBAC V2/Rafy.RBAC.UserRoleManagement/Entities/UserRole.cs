/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161201
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161201 10:20
 * 修改文件 宋军瑞 20161209 09:49 格式化代码
 * 
*******************************************************/

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Rafy.Accounts;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.RBAC.RoleManagement;

namespace Rafy.RBAC.UserRoleManagement
{
    /// <summary>
    /// 用户角色
    /// </summary>
    [ChildEntity]
    public class UserRole : UserRoleManagementEntity
    {
        #region 引用属性

        public static readonly Property<int> UserIdProperty = P<UserRole>.Register(e => e.UserId, ReferenceType.Parent);
        public long UserId
        {
            get { return (long) GetRefId(UserIdProperty); }
            set { SetRefId(UserIdProperty, value); }
        }
        public static readonly RefEntityProperty<User> UserProperty = P<UserRole>.RegisterRef(e => e.User, UserIdProperty);
        public User User
        {
            get { return GetRefEntity(UserProperty); }
            set { SetRefEntity(UserProperty, value); }
        }
        public static readonly Property<int> RoleIdProperty = P<UserRole>.Register(e => e.RoleId);
        public long RoleId
        {
            get { return (long) GetRefId(RoleIdProperty); }
            set { SetRefId(RoleIdProperty, value); }
        }

        public static readonly RefEntityProperty<Role> RoleProperty = P<UserRole>.RegisterRef(e => e.Role, RoleIdProperty);
        public Role Role
        {
            get { return GetRefEntity(RoleProperty); }
            set { SetRefEntity(RoleProperty, value); }
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
    /// 用户角色 列表类。
    /// </summary>
    public partial class UserRoleList : UserRoleManagementEntityList
    {
    }

    /// <summary>
    /// 用户角色 仓库类。
    /// 负责 实体的领域名称 类的查询、保存。
    /// </summary>
    public partial class UserRoleRepository : UserRoleManagementEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected UserRoleRepository()
        {
        }
    }

    /// <summary>
    /// 用户角色 配置类。
    /// 负责 实体的领域名称 类的实体元数据的配置。
    /// </summary>
    internal class UserRoleConfig : UserRoleManagementEntityConfig<UserRole>
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