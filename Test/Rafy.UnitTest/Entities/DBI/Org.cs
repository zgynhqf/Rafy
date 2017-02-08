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

namespace UT
{
    /// <summary>
    /// 组织树形实体
    /// </summary>
    [RootEntity, Serializable]
    public partial class Org : UnitTestEntity
    {
        #region 构造函数

        public Org() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Org(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<Org>.Register(e => e.Name);
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
    /// 组织树形实体 列表类。
    /// </summary>
    [Serializable]
    public partial class OrgList : UnitTestEntityList { }

    /// <summary>
    /// 组织树形实体 仓库类。
    /// 负责 组织树形实体 类的查询、保存。
    /// </summary>
    public partial class OrgRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected OrgRepository() { }
    }

    /// <summary>
    /// 组织树形实体 配置类。
    /// 负责 组织树形实体 类的实体元数据的配置。
    /// </summary>
    internal class OrgConfig : UnitTestEntityConfig<Org>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            //配置实体的所有属性都映射到数据表中。
            Meta.MapTable().MapAllProperties();
            Meta.EnablePhantoms();
            Meta.SupportTree();
        }
    }
}