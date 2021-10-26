/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130930
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130930 14:58
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.MultiLanguages;
using Rafy.Web;

namespace Rafy.MultiLanguages.WPF.ViewConfig
{
    internal class DevLanguageItemConfig : WPFViewConfig<DevLanguageItem>
    {
        protected override void ConfigView()
        {
            View.DomainName("开发语言项").HasDelegate(DevLanguageItem.ContentProperty);

            View.DisableEditing();

            //只能删除、更新
            View.UseCommands(
                WPFCommandNames.Delete,
                WPFCommandNames.SaveList,
                WPFCommandNames.Cancel,
                WPFCommandNames.Refresh,
                typeof(AutoRefresh),
                WPFCommandNames.Filter
                );

            using (View.OrderProperties())
            {
                View.Property(DevLanguageItem.ContentProperty).HasLabel("内容")
                    .ShowInList(gridWidth: 400).Readonly()
                    .UseEditor(WPFEditorNames.Memo);
            }
        }
    }
}