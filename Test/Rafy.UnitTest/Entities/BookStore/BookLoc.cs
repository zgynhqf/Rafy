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
using Rafy.Domain.ORM.Query;

namespace UT
{
    /// <summary>
    /// 书籍货架
    /// </summary>
    [RootEntity]
    public partial class BookLoc : UnitTestEntity
    {
        #region 引用属性

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> CodeProperty = P<BookLoc>.Register(e => e.Code);
        public string Code
        {
            get { return this.GetProperty<string>(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<BookLoc>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty<string>(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<int> LengthProperty = P<BookLoc>.Register(e => e.Length);
        /// <summary>
        /// 长度。
        /// </summary>
        public int Length
        {
            get { return this.GetProperty<int>(LengthProperty); }
            set { this.SetProperty(LengthProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 书籍货架 列表类。
    /// </summary>
    public partial class BookLocList : UnitTestEntityList { }

    /// <summary>
    /// 书籍货架 仓库类。
    /// 负责 书籍货架 类的查询、保存。
    /// </summary>
    public partial class BookLocRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected BookLocRepository() { }

        [RepositoryQuery]
        public virtual BookLocList Get_DAInRepository(string name)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.Name == name);
            return (BookLocList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual BookLocList Get_DAInDataProvider(string name)
        {
            throw new NotImplementedException("这个方法会在数据层中以同名方法实现。");
            //return this.DataProvider.Get_DAInDataProvider(name);
        }

        private new BookLocRepositoryDataProvider DataProvider
        {
            get { return base.DataProvider as BookLocRepositoryDataProvider; }
        }
    }

    [DataProviderFor(typeof(BookLocRepository))]
    public partial class BookLocRepositoryDataProvider : UnitTestEntityRepositoryDataProvider
    {
        public int TestSaveListTransactionItemCount = -1;

        public BookLocList Get_DAInDataProvider(string name)
        {
            var q = this.CreateLinqQuery<BookLoc>();
            q = q.Where(e => e.Name == name);
            return (BookLocList)this.QueryData(q);
        }

        protected override void Submit(SubmitArgs e)
        {
            if (TestSaveListTransactionItemCount >= 0)
            {
                TestSaveListTransactionItemCount++;
                if (TestSaveListTransactionItemCount > 1)
                {
                    throw new NotSupportedException("超过一条数据，直接抛出异常。之前的数据需要回滚。");
                }
            }

            base.Submit(e);
        }

        protected override void OnQuerying(ORMQueryArgs args)
        {
            var f = QueryFactory.Instance;
            var q = args.Query;
            q.Where = f.And(q.MainTable.Column(BookLoc.LengthProperty).GreaterEqual(0), q.Where);

            base.OnQuerying(args);
        }
    }

    /// <summary>
    /// 书籍货架 配置类。
    /// 负责 书籍货架 类的实体元数据、界面元数据的配置。
    /// </summary>
    internal class BookLocConfig : UnitTestEntityConfig<BookLoc>
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