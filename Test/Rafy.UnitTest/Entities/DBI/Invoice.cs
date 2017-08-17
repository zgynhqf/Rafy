/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151024
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151024 12:04
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

namespace UT
{
    /// <summary>
    /// 发票
    /// </summary>
    [RootEntity, Serializable]
    public partial class Invoice : UnitTestEntity
    {
        #region 构造函数

        public Invoice() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Invoice(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        #endregion

        #region 组合子属性

        public static readonly ListProperty<InvoiceItemList> InvoiceItemListProperty = P<Invoice>.RegisterList(e => e.InvoiceItemList);
        public InvoiceItemList InvoiceItemList
        {
            get { return this.GetLazyList(InvoiceItemListProperty); }
        }

        #endregion

        #region 一般属性

        public static readonly Property<string> CodeProperty = P<Invoice>.Register(e => e.Code);
        /// <summary>
        /// 发票编码
        /// </summary>
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 发票 列表类。
    /// </summary>
    [Serializable]
    public partial class InvoiceList : UnitTestEntityList { }

    /// <summary>
    /// 发票 仓库类。
    /// 负责 发票 类的查询、保存。
    /// </summary>
    public partial class InvoiceRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected InvoiceRepository() { }

        /// <summary>
        /// 返回所有明细的单据
        /// </summary>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual InvoiceList GetInvoice()
        {
            var f = QueryFactory.Instance;
            var t = f.Table<Invoice>();
            var t1 = f.Table<InvoiceItem>();
            var q = f.Query(
                selection: t.Star(),//查询所有列
                from: t.Join(t1, t.Column(Entity.IdProperty).Equal(t1.Column(InvoiceItem.InvoiceIdProperty)))//要查询的实体的表
            );
            return (InvoiceList)this.QueryData(q);
        }

        /// <summary>
        /// 返回有明细的单据列表
        /// </summary>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual InvoiceList GetInvoiceByHasItem()
        {
            var f = QueryFactory.Instance;
            var t = f.Table<Invoice>();
            var t1 = f.Table<InvoiceItem>();
            var q = f.Query(
                selection: t.Star(),//查询所有列
                from: t,//要查询的实体的表
                where: f.Exists(f.Query(
                    from: t1,
                    where: t1.Column(InvoiceItem.InvoiceIdProperty).Equal(t.Column(Entity.IdProperty))
            ))
            );
            return (InvoiceList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual LiteDataTable GetAllTable()
        {
            var sql = @"select * from invoice ";
            return (this.DataQueryer as RdbDataQueryer).QueryTable(sql);
        }
    }

    /// <summary>
    /// 发票 配置类。
    /// 负责 发票 类的实体元数据的配置。
    /// </summary>
    internal class InvoiceConfig : UnitTestEntityConfig<Invoice>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            //配置实体的所有属性都映射到数据表中。
            Meta.MapTable().MapAllProperties();

            Meta.EnablePhantoms();
        }
    }
}