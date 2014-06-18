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
    internal class MappingInfoConfig : WPFViewConfig<MappingInfo>
    {
        protected override void ConfigView()
        {
            View.DomainName("语言下的映射信息").HasDelegate(MappingInfo.DevLanguageRDProperty);

            View.UseCommands(typeof(FilterNotTranslatedItems), WPFCommandNames.Filter);

            using (View.OrderProperties())
            {
                View.Property(MappingInfo.DevLanguageRDProperty).HasLabel("原文")
                    .ShowInList(gridWidth: 300).Readonly();
                //.UseEditor(WPFEditorNames.Memo);
                View.Property(MappingInfo.TranslatedTextProperty).HasLabel("译文")
                    .ShowInList(gridWidth: 400);
                //.UseEditor(WPFEditorNames.Memo);
            }
        }
    }
}