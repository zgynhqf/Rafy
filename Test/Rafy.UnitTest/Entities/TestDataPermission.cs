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
using Rafy.RBAC.GroupManagement;
using UT;

namespace Rafy.UnitTest
{
    /// <summary>
    /// 测试数据权限
    /// </summary>
    [RootEntity]
    public partial class TestDataPermission : UnitTestEntity
    {
        #region 引用属性

        public static readonly IRefIdProperty GroupIdProperty =
     P<TestDataPermission>.RegisterRefId(e => e.GroupId, ReferenceType.Parent);
        /// <summary>
        /// 组的主键
        /// </summary>
        public long GroupId
        {
            get { return (long)this.GetRefId(GroupIdProperty); }
            set { this.SetRefId(GroupIdProperty, value); }
        }

        public static readonly RefEntityProperty<Group> GroupProperty =
            P<TestDataPermission>.RegisterRef(e => e.Group, GroupIdProperty);
        /// <summary>
        /// 组的关联对象
        /// </summary>
        public Group Group
        {
            get { return this.GetRefEntity(GroupProperty); }
            set { this.SetRefEntity(GroupProperty, value); }
        }

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性
        public static readonly Property<string> NameProperty = P<TestDataPermission>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 测试数据权限 列表类。
    /// </summary>
    public partial class TestDataPermissionList : UnitTestEntityList { }

    /// <summary>
    /// 测试数据权限 仓库类。
    /// 负责 测试数据权限 类的查询、保存。
    /// </summary>
    public partial class TestDataPermissionRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected TestDataPermissionRepository() { }
    }

    [DataProviderFor(typeof(TestDataPermissionRepository))]
    public partial class TestDataPermissionRepositoryDataProvider : RdbDataProvider
    {

        private static string _dbSettingName;
        /// <summary>
        /// 本插件中所有实体对应的连接字符串的配置名。
        /// 如果没有设置，则默认使用 <see cref="DbSettingNames.RafyPlugins"/>。
        /// </summary>
        public static string DbSettingName
        {
            get { return _dbSettingName ?? DbSettingNames.RafyPlugins; }
            set { _dbSettingName = value; }
        }

        protected override string ConnectionStringSettingName
        {
            get { return DbSettingName; }
        }
    }

    /// <summary>
    /// 测试数据权限 配置类。
    /// 负责 测试数据权限 类的实体元数据的配置。
    /// </summary>
    internal class TestDataPermissionConfig : UnitTestEntityConfig<TestDataPermission>
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