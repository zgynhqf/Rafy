/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2009
 * 
*******************************************************/

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
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.Module;
using OEA.Module.WPF;
using OEA.Module.WPF.Controls;
using OEA.Module.WPF.Editors;

namespace OEA.WPF.Command
{
    /// <summary>
    /// 此命令只能在导航查询面板视图 QueryObjectView 中使用
    /// </summary>
    [Command(Label = "查询", ToolTip = "查询记录", GroupType = CommandGroupType.System)]
    public class QueryObjectCommand : ClientCommand<QueryObjectView>
    {
        public override void Execute(QueryObjectView queryView)
        {
            var brokenRules = queryView.Current.ValidationRules.Validate();
            if (brokenRules.Count > 0)
            {
                App.MessageBox.Show(brokenRules.ToString(), "条件错误", //brokenRules[0].Description,
                    MessageBoxButton.OK);
            }
            else
            {
                queryView.TryExecuteQuery();
            }
        }
    }
}
