/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130821
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130821 15:29
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JXC.Commands;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace JXC.WPF.ViewConfig.BasicData
{
    internal class PurchaseOrderItemConfig : WPFViewConfig<PurchaseOrderItem>
    {
        protected override void ConfigView()
        {
            View.DomainName("商品订单项").HasDelegate(PurchaseOrderItem.View_ProductNameProperty);

            View.UseCommands(
                typeof(AddPurchaseOrderItem),
                WPFCommandNames.Delete
                );

            using (View.OrderProperties())
            {
                View.Property(PurchaseOrderItem.View_ProductNameProperty).HasLabel("商品名称").ShowIn(ShowInWhere.List);
                View.Property(PurchaseOrderItem.View_ProductCategoryNameProperty).HasLabel("商品类别").ShowIn(ShowInWhere.List);
                View.Property(PurchaseOrderItem.View_SpecificationProperty).HasLabel("规格").ShowIn(ShowInWhere.List);
                View.Property(PurchaseOrderItem.AmountProperty).HasLabel("数量").ShowIn(ShowInWhere.List);
                View.Property(PurchaseOrderItem.RawPriceProperty).HasLabel("单价").ShowIn(ShowInWhere.List);
                View.Property(PurchaseOrderItem.View_TotalPriceProperty).HasLabel("总价").ShowIn(ShowInWhere.List);
                View.Property(PurchaseOrderItem.AmountLeftProperty).HasLabel("未入库数量").ShowIn(ShowInWhere.List).Readonly();
            }
        }
    }
}