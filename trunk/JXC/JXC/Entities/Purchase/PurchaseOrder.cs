/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120416
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120416
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;

namespace JXC
{
    [RootEntity, Serializable]
    [ConditionQueryType(typeof(PurchaseOrderCriteria))]
    public class PurchaseOrder : JXCEntity
    {
        public static readonly Property<string> NameProperty = P<PurchaseOrder>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
    }

    [Serializable]
    public class PurchaseOrderList : JXCEntityList { }

    public class PurchaseOrderRepository : EntityRepository
    {
        protected PurchaseOrderRepository() { }
    }

    internal class PurchaseOrderConfig : EntityConfig<PurchaseOrder>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().HasColumns(
                PurchaseOrder.NameProperty
                );
        }

        protected override void ConfigView()
        {
            View.DomainName("采购订单").HasDelegate(PurchaseOrder.NameProperty);

            View.Property(PurchaseOrder.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}