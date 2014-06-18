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
    internal class StorageProductConfig : WebViewConfig<StorageProduct>
    {
        protected override void ConfigView()
        {
            View.DomainName("库存货品").HasDelegate(StorageProduct.View_ProductNameProperty);

            View.UseDefaultCommands();

            using (View.OrderProperties())
            {
                View.Property(StorageProduct.View_ProductNameProperty).HasLabel("商品名称").ShowIn(ShowInWhere.All);
                View.Property(StorageProduct.View_ProductCategoryNameProperty).HasLabel("商品类别").ShowIn(ShowInWhere.List);
                View.Property(StorageProduct.View_SpecificationProperty).HasLabel("规格").ShowIn(ShowInWhere.List);
                View.Property(StorageProduct.AmountProperty).HasLabel("当前数量").ShowIn(ShowInWhere.List);
            }
        }
    }
}