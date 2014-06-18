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
    internal class StorageConfig : WebViewConfig<Storage>
    {
        protected override void ConfigView()
        {
            View.DomainName("仓库").HasDelegate(Storage.NameProperty);

            View.UseDefaultCommands();

            using (View.OrderProperties())
            {
                View.Property(Storage.CodeProperty).HasLabel("仓库编码").ShowIn(ShowInWhere.All);
                View.Property(Storage.NameProperty).HasLabel("仓库名称").ShowIn(ShowInWhere.All);
                View.Property(Storage.AddressProperty).HasLabel("仓库地址").ShowIn(ShowInWhere.ListDetail);
                View.Property(Storage.ResponsiblePersonProperty).HasLabel("负责人").ShowIn(ShowInWhere.ListDetail);
                View.Property(Storage.AreaProperty).HasLabel("仓库区域").ShowIn(ShowInWhere.ListDetail);
                View.Property(Storage.IsDefaultProperty).HasLabel("默认仓库").ShowIn(ShowInWhere.List);
            }
        }
    }
}