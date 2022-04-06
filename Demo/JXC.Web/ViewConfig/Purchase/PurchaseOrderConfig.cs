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
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace JXC.Web.ViewConfig.BasicData
{
    internal class PurchaseOrderConfig : WebViewConfig<PurchaseOrder>
    {
        protected override void ConfigView()
        {
            View.DomainName("采购订单").HasDelegate(PurchaseOrder.CodeProperty);

            View.UseCommands(
                "Jxc.AddPurchaseOrder",
                "Jxc.ShowBill",
                WebCommandNames.Refresh
                );

            using (View.OrderProperties())
            {
                View.Property(PurchaseOrder.CodeProperty).HasLabel("订单编号").ShowIn(ShowInWhere.All);
                View.Property(PurchaseOrder.DateProperty).HasLabel("订单日期").ShowIn(ShowInWhere.ListDetail);
                //View.Property(PurchaseOrder.SupplierNameProperty).HasLabel("供应商").ShowIn(ShowInWhere.List);
                View.Property(PurchaseOrder.SupplierProperty).HasLabel("供应商").ShowIn(ShowInWhere.ListDetail)
                    .UseDataSource(EntityDataSources.Suppliers);
                View.Property(PurchaseOrder.SupplierCategoryNameProperty).HasLabel("供应商客户类别").ShowIn(ShowInWhere.List);
                View.Property(PurchaseOrder.PlanStorageInDateProperty).HasLabel("计划到货日期").ShowIn(ShowInWhere.ListDetail);
                View.Property(PurchaseOrder.StorageInDirectlyProperty).HasLabel("直接入库").ShowIn(ShowInWhere.Detail);
                View.Property(PurchaseOrder.TotalMoneyProperty).HasLabel("总金额").ShowIn(ShowInWhere.ListDetail)
                    .Readonly();
                View.Property(PurchaseOrder.StorageProperty).HasLabel("仓库").ShowIn(ShowInWhere.Detail)
                    .Visibility(PurchaseOrder.StorageInDirectlyProperty);//动态只读//.Readonly(PurchaseOrder.IsStorageReadonlyProperty);//动态只读
                View.Property(PurchaseOrder.StorageInStatusProperty).HasLabel("入库状态").ShowIn(ShowInWhere.List)
                    .Readonly();
                View.Property(PurchaseOrder.TotalAmountLeftProperty).HasLabel("未入库商品数").ShowIn(ShowInWhere.List)
                    .Readonly();
                View.Property(PurchaseOrder.CommentProperty).HasLabel("备注").ShowIn(ShowInWhere.ListDetail);
            }
        }
    }
}