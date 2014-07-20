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
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Controls;
using Rafy.WPF.Editors;

namespace Rafy.WPF.Command
{
    /// <summary>
    /// 此命令只能在导航查询面板视图 QueryLogicalView 中使用
    /// </summary>
    [Command(Label = "查询", ToolTip = "查询记录", GroupType = CommandGroupType.System, Gestures = "F5", ImageName = "NewSearch.png")]
    public class QueryObjectCommand : ClientCommand<QueryLogicalView>
    {
        public override void Execute(QueryLogicalView queryView)
        {
            var brokenRules = queryView.Current.Validate();
            if (brokenRules.Count > 0)
            {
                App.MessageBox.Show(brokenRules.ToString(), "条件错误".Translate(), //brokenRules[0].Description,
                    MessageBoxButton.OK);
            }
            else
            {
                queryView.TryExecuteQuery();
            }
        }
    }
}