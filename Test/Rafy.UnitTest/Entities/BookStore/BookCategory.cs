using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Data;
using Rafy.Domain.ORM;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;

namespace UT
{
    /// <summary>
    /// 书籍类别
    /// </summary>
    [RootEntity, Serializable]
    public partial class BookCategory : UnitTestEntity
    {
        #region 构造函数

        public BookCategory() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected BookCategory(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<BookCategory>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> CodeProperty = P<BookCategory>.Register(e => e.Code);
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        //public static readonly Property<DateTime> CreateTimeProperty = P<BookCategory>.Register(e => e.CreateTime);
        ///// <summary>
        ///// 类别创建的时间
        ///// </summary>
        //public DateTime CreateTime
        //{
        //    get { return this.GetProperty(CreateTimeProperty); }
        //    set { this.SetProperty(CreateTimeProperty, value); }
        //}

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 书籍类别 列表类。
    /// </summary>
    [Serializable]
    public partial class BookCategoryList : UnitTestEntityList { }

    /// <summary>
    /// 书籍类别 仓库类。
    /// 负责 书籍类别 类的查询、保存。
    /// </summary>
    public partial class BookCategoryRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected BookCategoryRepository() { }
    }

    /// <summary>
    /// 书籍类别 配置类。
    /// 负责 书籍类别 类的实体元数据、界面元数据的配置。
    /// </summary>
    internal class BookCategoryConfig : UnitTestEntityConfig<BookCategory>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            //配置实体的所有属性都映射到数据表中。
            Meta.MapTable().MapAllProperties();
        }

        protected override void AddValidations(IValidationDeclarer rules)
        {
            rules.AddRule(new NotUsedByReferenceRule(Book.BookCategoryIdProperty), new RuleMeta
            {
                Scope = EntityStatusScopes.Delete
            });
            base.AddValidations(rules);
        }
    }
}