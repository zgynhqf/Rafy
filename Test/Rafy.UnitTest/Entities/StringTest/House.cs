/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140516
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140516 10:06
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
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
    /// 
    /// </summary>
    [RootEntity]
    public partial class House : StringTestEntity
    {
        #region 引用属性

        public static readonly IRefIdProperty LesseeIdProperty =
            P<House>.RegisterRefId(e => e.LesseeId, ReferenceType.Normal);
        public long? LesseeId
        {
            get { return (long?)this.GetRefNullableId(LesseeIdProperty); }
            set { this.SetRefNullableId(LesseeIdProperty, value); }
        }
        public static readonly RefEntityProperty<Lessee> LesseeProperty =
            P<House>.RegisterRef(e => e.Lessee, LesseeIdProperty);
        /// <summary>
        /// 当前租户
        /// </summary>
        public Lessee Lessee
        {
            get { return this.GetRefEntity(LesseeProperty); }
            set { this.SetRefEntity(LesseeProperty, value); }
        }

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<House>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion

        public bool IdChanged = false;
        protected override void OnIdChanged(ManagedPropertyChangedEventArgs e)
        {
            base.OnIdChanged(e);
            IdChanged = true;
        }
    }

    /// <summary>
    ///  列表类。
    /// </summary>
    public partial class HouseList : StringTestEntityList { }

    /// <summary>
    ///  仓库类。
    /// 负责  类的查询、保存。
    /// </summary>
    public partial class HouseRepository : StringTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected HouseRepository() { }
    }

    /// <summary>
    ///  配置类。
    /// 负责  类的实体元数据的配置。
    /// </summary>
    internal class HouseConfig : StringTestEntityConfig<House>
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