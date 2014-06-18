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
    /// 只删除块的命令。
    /// </summary>
    public class DeleteBlockCommand : WPFDesignerCommand
    {
        public static RoutedUICommand WPFCommand = new RoutedUICommand("删除块", "DeleteBlock", typeof(DeleteBlockCommand),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Delete)
            });

        protected override void ExecuteCore(ModelingDesigner designer)
        {
            var selectedItems = designer.SelectedItems.ToArray();
            foreach (var item in selectedItems)
            {
                if (item.Kind == DesignerComponentKind.Block)
                {
                    designer.Blocks.Remove(item as BlockControl);
                }
            }
        }
    }
}