using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.Module.WPF;
using OEA.Module.WPF.CommandAutoUI;
using OEA.Module.WPF.Controls;
using OEA.WPF.Command;
using System.Windows;

namespace FM.Commands
{
    [Command(Label = "添加", ToolTip = "Ctrl+回车 直接添加", GroupType = CommandGroupType.Business)]
    public class ContinueAddFinanceLog : ClientCommand<DetailObjectView>
    {
        public override void Execute(DetailObjectView view)
        {
            var log = view.Current.CastTo<FinanceLog>();
            var brokenRules = log.ValidationRules.Validate();
            if (brokenRules.Count > 0)
            {
                App.MessageBox.Show(brokenRules.ToString(), "添加出错");
                return;
            }

            RF.Save(log);

            var listView = view.TryFindRelation("list");
            listView.DataLoader.ReloadDataAsync();

            App.MessageBox.Show("添加成功。");

            log.MarkNew();
            log.Amount = 0;

            //定位焦点到数量上
            var element = view.FindPropertyEditor(FinanceLog.AmountProperty).Control as FrameworkElement;
            element.Focus();
        }
    }
}