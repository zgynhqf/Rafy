using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using SimpleCsla;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module.View;

namespace OEA.Module.WPF.Behaviors
{
    public class TreeSelectViewBehavior : ViewBehavior
    {
        /// <summary>
        /// 树形关联级别类型
        /// </summary>
        private enum AssociatedMode
        {
            //不关联，仅选择自己
            None,
            //关联儿子
            Son,
            //级联所有子孙
            All
        }

        private static bool _ignoreEvent;
        private static AssociatedMode _associatedMode = 0;

        private void InitControl()
        {
            RadioButton rdbtnNone = new RadioButton();
            rdbtnNone.Content = "不关联";
            RadioButton rdbtnSon = new RadioButton();
            rdbtnSon.Content = "关联子";
            RadioButton rdbtnAll = new RadioButton();
            rdbtnAll.Content = "关联全部";
            rdbtnNone.Checked += new RoutedEventHandler(rdbtnNone_Checked);
            rdbtnSon.Checked += new RoutedEventHandler(rdbtnSon_Checked);
            rdbtnAll.Checked += new RoutedEventHandler(rdbtnAll_Checked);
            var tbMain = (this.View as WPFObjectView).CommandsContainer;
            Debug.Assert(tbMain != null, "没有生成控件工具栏");
            DockPanel dpnl = new DockPanel();
            dpnl.Children.Add(rdbtnNone);
            dpnl.Children.Add(rdbtnSon);
            dpnl.Children.Add(rdbtnAll);
            tbMain.Items.Add(dpnl);
            //默认不关联
            rdbtnNone.IsChecked = true;

            tbMain.Visibility = Visibility.Visible;
        }

        private void rdbtnAll_Checked(object sender, RoutedEventArgs e)
        {
            _associatedMode = AssociatedMode.All;
        }

        private void rdbtnSon_Checked(object sender, RoutedEventArgs e)
        {
            _associatedMode = AssociatedMode.Son;
        }

        private void rdbtnNone_Checked(object sender, RoutedEventArgs e)
        {
            _associatedMode = AssociatedMode.None;
        }

        private void View_DataChanged(object sender, EventArgs e)
        {
            DealOnSelecteionChanged((sender as ListObjectView).Data);
        }

        private void DealOnSelecteionChanged(IList displayList)
        {
            if (displayList == null) throw new ArgumentNullException("dislplaylist");
            var treeList = displayList as EntityList;

            foreach (IDisplayModel srcItem in displayList)
            {
                srcItem.PropertyChanged -= new PropertyChangedEventHandler(srcItem_PropertyChanged);
                srcItem.PropertyChanged += new PropertyChangedEventHandler(srcItem_PropertyChanged);
            }
        }

        private static void srcItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.EqualsIgnorecase(PropertyConvention.IsSelected))
            {
                if (_ignoreEvent) return;
                _ignoreEvent = true;
                IDisplayModel displayModel = sender as IDisplayModel;
                var treeNode = sender as Entity;
                Debug.Assert(displayModel != null && treeNode != null);
                bool isSelected = displayModel.IsSelected;
                if (_associatedMode == AssociatedMode.Son)
                {
                    foreach (IDisplayModel childNode in treeNode.TreeChildren)
                    {
                        childNode.IsSelected = isSelected;
                    }
                }
                else if (_associatedMode == AssociatedMode.All)
                {
                    foreach (IDisplayModel childNode in treeNode.GetRecurChildren())
                    {
                        childNode.IsSelected = isSelected;
                    }
                }

                //选中的话递归选中父亲节点
                if (isSelected && _associatedMode != AssociatedMode.None)
                {
                    var node = treeNode.TreeParent;
                    while (node != null)
                    {
                        (node as IDisplayModel).IsSelected = true;
                        node = node.TreeParent;
                    }
                }
                _ignoreEvent = false;
            }
        }

        protected override void OnAttach()
        {
            this.View.DataChanged += new EventHandler(View_DataChanged);
            InitControl();
        }
    }
}
