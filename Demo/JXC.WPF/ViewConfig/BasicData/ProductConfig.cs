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
    internal class ProductConfig : WPFViewConfig<Product>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.DomainName("商品").HasDelegate(Product.MingChengProperty);

            View.UseDefaultCommands().UseCommands(typeof(ResetProductAmountCommand));

            View.UseDetailPanel<ProductForm>();

            using (View.OrderProperties())
            {
                View.Property(Product.BianMaProperty)
                    .HasLabel("编码").ShowIn(ShowInWhere.All).ShowInDetail(contentWidth: 0.7, columnSpan: 2);
                View.Property(Product.MingChengProperty)
                    .HasLabel("名称").ShowIn(ShowInWhere.All).ShowInDetail(contentWidth: 600, columnSpan: 2);
                View.Property(Product.BarcodeProperty)
                    .HasLabel("条码").ShowIn(ShowInWhere.ListDetail).ShowInDetail(contentWidth: 0.7, columnSpan: 2);
                View.Property(Product.PictureProperty)
                    .HasLabel("图片").ShowIn(ShowInWhere.Detail).UseEditor("ImageSelector");
                View.Property(Product.ProductCategoryProperty)
                    .HasLabel("商品类别").ShowIn(ShowInWhere.ListDetail);
                View.Property(Product.GuiGeProperty)
                    .HasLabel("规格").ShowIn(ShowInWhere.ListDetail);
                View.Property(Product.PingPaiProperty)
                    .HasLabel("品牌").ShowIn(ShowInWhere.ListDetail).ShowInDetail(columnSpan: 2);
                View.Property(Product.StorageAmountProperty)
                    .HasLabel("库存量").ShowIn(ShowInWhere.ListDetail).Readonly();
                View.Property(Product.CaiGouDanjiaProperty)
                    .HasLabel("采购单价").ShowIn(ShowInWhere.ListDetail);
                View.Property(Product.XiaoShouDanJiaProperty)
                    .HasLabel("销售单价").ShowIn(ShowInWhere.ListDetail);
                View.Property(Product.SupplierProperty)
                    .HasLabel("销售商名称").ShowIn(ShowInWhere.ListDetail);
                View.Property(Product.XiaoShouJia_1Property)
                    .HasLabel("一级销售价").ShowIn(ShowInWhere.ListDetail);
                View.Property(Product.XiaoShouJia_2Property)
                    .HasLabel("二级销售价").ShowIn(ShowInWhere.ListDetail);
                View.Property(Product.XiaoShouJia_3Property)
                    .HasLabel("三级销售价").ShowIn(ShowInWhere.ListDetail);
                View.Property(Product.BeiZhuProperty)
                    .HasLabel("备注").ShowIn(ShowInWhere.ListDetail).ShowMemoInDetail();
                View.Property(Product.OperateTimeProperty)
                    .HasLabel("操作时间").ShowIn(ShowInWhere.Detail).Readonly();
                View.Property(Product.OperatorProperty)
                    .HasLabel("操作员").ShowIn(ShowInWhere.Detail).Readonly();

                View.Property(Product.Time1Property)
                    .HasLabel("日期").ShowIn(ShowInWhere.ListDetail);
                View.Property(Product.Time2Property)
                    .HasLabel("日期时间").ShowIn(ShowInWhere.ListDetail);
                View.Property(Product.Time3Property)
                    .HasLabel("时间").ShowIn(ShowInWhere.ListDetail);
            }
        }
    }
}
