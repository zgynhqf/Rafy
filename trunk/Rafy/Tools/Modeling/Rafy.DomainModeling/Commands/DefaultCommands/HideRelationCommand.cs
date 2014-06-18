/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130414 10:03
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130414 10:03
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
    /// 隐藏/显示所有选择的关系线。
    /// </summary>
    public class HideRelationCommand : WPFDesignerCommand
    {
        public static RoutedUICommand WPFCommand = new RoutedUICommand("隐藏/显示所有选择的关系线", "HideRelation", typeof(HideRelationCommand),
            new InputGestureCollection()
            {
                new KeyGesture(Key.D1, ModifierKeys.Control)
            });

        protected override void ExecuteCore(ModelingDesigner designer)
        {
            foreach (var item in designer.SelectedItems)
            {
                if (item.Kind == DesignerComponentKind.Relation)
                {
                    var relation = item as BlockRelation;
                    relation.Hidden = !relation.Hidden;
                }
            }
        }
    }
}