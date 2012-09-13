/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120829 11:40
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120829 11:40
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace OEA.Module.WPF.Controls
{
    public partial class TreeGrid
    {
        public static readonly RoutedCommand CommitEditCommand = new RoutedCommand("CommitEdit", typeof(TreeGrid));

        static TreeGrid()
        {
            //此属性如果为真，GetChildItems 方法返回值必须实现 INotifyCollectionChanged 接口。
            //目前，object.ChildrenNodes 无法进行监听，所以我们选择不实现自动模式，而是直接调用 Refresh 接口。
            //相关 API 定义，请查看 ObserveChildItems 属性文档。
            ObserveChildItemsProperty.OverrideMetadata(typeof(TreeGrid), new FrameworkPropertyMetadata(false));
            KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeof(TreeGrid),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));

            CommandManager.RegisterClassCommandBinding(typeof(TreeGrid), new CommandBinding(CommitEditCommand,
                (o, e) => (o as TreeGrid).OnCommitEditCommandExecuted(e)));

            EventManager.RegisterClassHandler(typeof(TreeGrid), GridTreeViewColumnHeader.ClickEvent, (RoutedEventHandler)ColumnHeader_Click);
        }

        private static void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            (sender as TreeGrid).OnColumnHeaderClick(e);
        }

        protected virtual void OnCommitEditCommandExecuted(ExecutedRoutedEventArgs e)
        {
            //由于目前文本框都是即时绑定（UpdateSourceTrigger.PropertyChanged），所以暂时不需要再显式提交数据。
            e.Handled = true;
        }
    }
}