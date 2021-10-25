/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151024
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151024 12:07
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
    /// 发票明细
    /// </summary>
    [ChildEntity, Serializable]
    public partial class InvoiceItem : UnitTestEntity
    {
        #region 引用属性

        public static readonly IRefIdProperty InvoiceIdProperty =
            P<InvoiceItem>.RegisterRefId(e => e.InvoiceId, ReferenceType.Parent);
        public int InvoiceId
        {
            get { return (int)this.GetRefId(InvoiceIdProperty); }
            set { this.SetRefId(InvoiceIdProperty, value); }
        }
        public static readonly RefEntityProperty<Invoice> InvoiceProperty =
            P<InvoiceItem>.RegisterRef(e => e.Invoice, InvoiceIdProperty);
        public Invoice Invoice
        {
            get { return this.GetRefEntity(InvoiceProperty); }
            set { this.SetRefEntity(InvoiceProperty, value); }
        }

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<double> AmountProperty = P<InvoiceItem>.Register(e => e.Amount);
        /// <summary>
        /// 量
        /// </summary>
        public double Amount
        {
            get { return this.GetProperty(AmountProperty); }
            set { this.SetProperty(AmountProperty, value); }
        }

        public static readonly Property<bool> IsDefaultProperty = P<InvoiceItem>.Register(e => e.IsDefault);
        /// <summary>
        /// 是否默认
        /// </summary>
        public bool IsDefault
        {
            get { return this.GetProperty(IsDefaultProperty); }
            set { this.SetProperty(IsDefaultProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 发票明细 列表类。
    /// </summary>
    [Serializable]
    public partial class InvoiceItemList : UnitTestEntityList { }

    /// <summary>
    /// 发票明细 仓库类。
    /// 负责 发票明细 类的查询、保存。
    /// </summary>
    public partial class InvoiceItemRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected InvoiceItemRepository() { }
    }

    /// <summary>
    /// 发票明细 配置类。
    /// 负责 发票明细 类的实体元数据的配置。
    /// </summary>
    internal class InvoiceItemConfig : UnitTestEntityConfig<InvoiceItem>
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