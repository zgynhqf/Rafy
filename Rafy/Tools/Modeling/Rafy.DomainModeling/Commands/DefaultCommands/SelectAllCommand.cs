/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130412 14:12
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130412 14:12
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
    /// 全选命令。用于选择设计器中所有可选择的元素。
    /// </summary>
    public class SelectAllCommand : WPFDesignerCommand
    {
        public static RoutedUICommand WPFCommand = new RoutedUICommand("全选", "SelectAll", typeof(DeleteBlockCommand),
            new InputGestureCollection()
            {
                new KeyGesture(Key.A, ModifierKeys.Control)
            });

        protected override void ExecuteCore(ModelingDesigner designer)
        {
            designer.SelectAll();
        }
    }
}
