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
using System.Windows;
using System.Windows.Input;
using Rafy.DomainModeling.Controls;

namespace Rafy.DomainModeling.Commands
{
    /// <summary>
    /// 删除选择的块或连接
    /// </summary>
    public class DeleteCommand : WPFDesignerCommand
    {
        public static RoutedUICommand WPFCommand = new RoutedUICommand("删除块或连接", "Delete", typeof(DeleteCommand),
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

            //先删除所有的块后，如果还剩下选择的连接没有移除的话，说明用户想删除某个连接对应的关系，这时需要进行提示。
            selectedItems = designer.SelectedItems.ToArray();
            if (selectedItems.All(c => c.Kind == DesignerComponentKind.Relation))
            {
                var res = MessageBox.Show("确定删除关系？", "确认删除", MessageBoxButton.YesNo);
                if (res != MessageBoxResult.Yes) return;
            }

            foreach (var item in selectedItems)
            {
                designer.Relations.Remove(item as BlockRelation);
            }
        }
    }
}
