/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130414 09:50
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130414 09:50
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Rafy.DomainModeling.Controls;

namespace Rafy.DomainModeling.Commands
{
    /// <summary>
    /// 显示/隐藏连接线
    /// </summary>
    public class ToggleHiddenRelationsCommand : WPFDesignerCommand
    {
        public static RoutedUICommand WPFCommand = new RoutedUICommand("显示/隐藏连接线", "ToggleHiddenRelations", typeof(ToggleHiddenRelationsCommand),
            new InputGestureCollection()
            {
                new KeyGesture(Key.T, ModifierKeys.Control)
            });

        protected override void ExecuteCore(ModelingDesigner designer)
        {
            designer.ShowHiddenRelations = !designer.ShowHiddenRelations;
        }
    }
}