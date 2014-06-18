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
using Rafy;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.MultiLanguages;
using Rafy.Web;

namespace Rafy.MultiLanguages.WPF.ViewConfig
{
    internal class LanguageConfig : WPFViewConfig<Language>
    {
        protected override void ConfigView()
        {
            View.DomainName("语言").HasDelegate(Language.NameProperty);

            View.UseCommands(
                WPFCommandNames.PopupAdd,
                WPFCommandNames.Edit,
                WPFCommandNames.Delete,
                WPFCommandNames.SaveList,
                WPFCommandNames.Cancel,
                WPFCommandNames.Refresh,
                typeof(AutoRefresh),
                typeof(AutoTranslate_Baidu),
                typeof(AutoTranslate_Bing),
                typeof(OpenAllModules)
                );

            if (!ConfigurationHelper.GetAppSettingOrDefault("Rafy_LanguageManaging", false))
            {
                View.RemoveCommands(
                    WPFCommandNames.PopupAdd,
                    WPFCommandNames.Delete
                    );
            }

            using (View.OrderProperties())
            {
                View.Property(Language.CodeProperty).HasLabel("编码").ShowIn(ShowInWhere.All);
                View.Property(Language.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
                View.Property(Language.BingAPICodeProperty).HasLabel("翻译引擎编码").ShowIn(ShowInWhere.All);
                View.Property(Language.NeedCollectProperty).HasLabel("是否执行收集操作").ShowIn(ShowInWhere.All);
            }
        }
    }
}