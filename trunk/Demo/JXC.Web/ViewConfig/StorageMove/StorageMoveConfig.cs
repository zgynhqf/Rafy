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
    internal class StorageMoveConfig : WebViewConfig<StorageMove>
    {
        protected override void ConfigView()
        {
            View.DomainName("库存调拔").HasDelegate(StorageMove.CodeProperty);

            View.HasDetailColumnsCount(2);

            using (View.OrderProperties())
            {
                View.Property(StorageMove.CodeProperty).HasLabel("调拔单编号").ShowIn(ShowInWhere.All);
                View.Property(StorageMove.UserProperty).HasLabel("发货人").ShowIn(ShowInWhere.ListDetail);
                View.Property(StorageMove.DateProperty).HasLabel("发货日期").ShowIn(ShowInWhere.ListDetail);
                View.Property(StorageMove.StorageFromProperty).HasLabel("出货仓库").ShowIn(ShowInWhere.ListDetail);
                View.Property(StorageMove.StorageToProperty).HasLabel("收货仓库").ShowIn(ShowInWhere.ListDetail);
                View.Property(StorageMove.CommentProperty).HasLabel("备注").ShowIn(ShowInWhere.ListDetail);
            }
        }
    }
}