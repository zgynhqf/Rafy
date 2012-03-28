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
using SimpleCsla;
using SimpleCsla.Core;
using SimpleCsla.OEA;
using SimpleCsla.Wpf;
using OEA.Command;
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
    /// View为NavigateQueryObjectView
    /// </summary>
    [Command(Label = "查询", ToolTip = "查询记录")]
    public class QueryObjectCommand : ViewCommand
    {
        public override void Execute(ObjectView view)
        {
            var queryView = view as QueryObjectView;
            Debug.Assert(queryView != null, "此命令只能在导航查询面板视图QueryObjectView中使用");

            var queryObject = queryView.Current;
            queryObject.CheckRules();
            if (queryObject.BrokenRulesCollection.Count > 0)
            {
                App.Current.MessageBox.Show("条件错误", queryObject.BrokenRulesCollection[0].Description, MessageBoxButton.OK);
            }
            else
            {
                var resultView = queryView.ResultView;
                this.QueryData(resultView, queryObject);
            }
        }

        protected virtual void QueryData(ObjectView resultView, Criteria queryObject)
        {
            resultView.DataLoader.GetObjectAsync(queryObject);
        }
    }
}
