/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121107 23:25
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121107 23:25
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using Rafy;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Command;
using Rafy.WPF.Controls;

namespace Rafy.MultiLanguages.WPF
{
    [Command(Label = "自动更新", ToolTip = "更新本次运行收集的所有语言信息", GroupType = CommandGroupType.Business)]
    public class AutoRefresh : ListViewCommand
    {
        public override void Execute(ListLogicalView view)
        {
            DbTranslator.Instance.AutoSave();

            if (view.Data == null || !view.Data.IsDirty)
            {
                view.DataLoader.LoadDataAsync();
            }

            App.MessageBox.Show("更新完成。".Translate());
        }
    }

    [Command(Label = "翻译引擎1", GroupType = CommandGroupType.Business)]
    public class AutoTranslate_Bing : AutoTranslateCommandBase
    {
        public AutoTranslate_Bing()
        {
            this.Engine = new Bing();
        }
    }

    [Command(Label = "翻译引擎2", GroupType = CommandGroupType.Business)]
    public class AutoTranslate_Baidu : AutoTranslateCommandBase
    {
        public AutoTranslate_Baidu()
        {
            this.Engine = new Baidu();
        }
    }

    public class AutoTranslateCommandBase : ListViewCommand
    {
        protected TranslationEngine Engine;

        public override bool CanExecute(ListLogicalView view)
        {
            var language = view.Current as Language;
            return language != null && !string.IsNullOrEmpty(language.BingAPICode);
        }

        public override void Execute(ListLogicalView view)
        {
            var language = view.Current as Language;
            AsyncAction.Execute(() =>
            {
                TranslateByEngine(language);
            });
        }

        private void TranslateByEngine(Language language)
        {
            var toDoList = language.MappingInfoList.Cast<MappingInfo>()
                .Where(m => string.IsNullOrEmpty(m.TranslatedText))
                .ToList();

            this.Engine.Translate(language, toDoList);
        }
    }

    [Command(Label = "打开所有模块", ToolTip = "打开所有模块以方便收集所有字符串", GroupType = CommandGroupType.Business)]
    public class OpenAllModules : ViewCommand
    {
        public override void Execute(LogicalView view)
        {
            var res = App.MessageBox.Show("打开所有模块需要一定时间，确定吗？".Translate(), MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                var modules = App.Current.UserRootModules;
                modules.TravalTree(vm =>
                {
                    if (vm.Type == ModuleViewModelType.EntityModule) { vm.IsSelected = true; }
                    return false;
                });
            }
        }
    }

    [Command(Label = "过滤未翻译项", GroupType = CommandGroupType.Business)]
    public class FilterNotTranslatedItems : ListViewCommand
    {
        public override void Execute(ListLogicalView view)
        {
            view.Filter = e => string.IsNullOrEmpty((e as MappingInfo).TranslatedText);
        }
    }
}