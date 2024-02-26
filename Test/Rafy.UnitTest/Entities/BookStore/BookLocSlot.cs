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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using static Rafy.Domain.ORM.Query.FactoryMethods;

namespace UT
{
    /// <summary>
    /// 书籍货架位置
    /// </summary>
    [ChildEntity]
    public partial class BookLocSlot : UnitTestEntity
    {
        #region 引用属性

        public static readonly Property<string> BookLocCodeProperty = P<BookLocSlot>.Register(e => e.BookLocCode);
        /// <summary>
        /// 
        /// </summary>
        public string BookLocCode
        {
            get { return this.GetProperty(BookLocCodeProperty); }
            set { this.SetProperty(BookLocCodeProperty, value); }
        }

        public static readonly RefEntityProperty<BookLoc> BookLocProperty =
            P<BookLocSlot>.RegisterRef(e => e.BookLoc, BookLocCodeProperty, BookLoc.CodeProperty, ReferenceType.Parent);
        /// <summary>
        /// $end$
        /// </summary>
        public BookLoc BookLoc
        {
            get { return this.GetRefEntity(BookLocProperty); }
            set { this.SetRefEntity(BookLocProperty, value); }
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
    /// 书籍货架位置 列表类。
    /// </summary>
    public partial class BookLocSlotList : UnitTestEntityList { }

    /// <summary>
    /// 书籍货架位置 仓库类。
    /// 负责 书籍货架位置 类的查询、保存。
    /// </summary>
    public partial class BookLocSlotRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected BookLocSlotRepository() { }
    }

    /// <summary>
    /// 书籍货架位置 配置类。
    /// 负责 书籍货架位置 类的实体元数据的配置。
    /// </summary>
    internal class BookLocSlotConfig : UnitTestEntityConfig<BookLocSlot>
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