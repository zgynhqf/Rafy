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
    internal class FileAttachementConfig : WebViewConfig<FileAttachement>
    {
        protected override void ConfigView()
        {
            View.DomainName("附件").HasDelegate(FileAttachement.FileNameProperty);

            using (View.OrderProperties())
            {
                View.Property(FileAttachement.FileNameProperty).HasLabel("文件名").ShowIn(ShowInWhere.All);
                View.Property(FileAttachement.UploadDateProperty).HasLabel("创建日期").ShowIn(ShowInWhere.ListDetail);
            }
        }
    }
}
