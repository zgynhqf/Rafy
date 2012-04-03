using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using AvalonDock;

using OEA.Core;



using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.MetaModel.Attributes;
using OEA.Module;
using OEA.Module.WPF;
using OEA.Module.WPF.Editors;
using OEA.Module.WPF.Controls;

namespace OEA.WPF.Command
{
    /// <summary>
    /// 此命令只能在导航查询面板视图 QueryObjectView 中使用
    /// </summary>
    [Command(Label = "查询", ToolTip = "查询记录")]
    public class QueryObjectCommand : ClientCommand<QueryObjectView>
    {
        public override void Execute(QueryObjectView queryView)
        {
            var brokenRules = queryView.Current.ValidationRules.CheckRules();
            if (brokenRules.Count > 0)
            {
                App.Current.MessageBox.Show(
                    "条件错误",
                    brokenRules.ToString(),
                    //brokenRules[0].Description,
                    MessageBoxButton.OK
                    );
            }
            else
            {
                queryView.TryExecuteQuery();
            }
        }
    }
}
