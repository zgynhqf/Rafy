using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using OEA.Module.WPF.Controls;
using Itenso.Windows.Input;
using OEA.MetaModel;
using OEA.MetaModel.View;
using System.Windows.Automation;

using System.Windows;
using System.ComponentModel;
using OEA.WPF.Command;

namespace OEA.Module.WPF.Layout
{
    /// <summary>
    /// 传统布局
    /// 布局是一个 ContentControl，同时，在它里面可以安排以下内容：
    /// 
    /// Main, Toolbar, Navigate, Condition, Result, List, Detail, Children
    /// </summary>
    public class TraditionalLayout : ContentControl
    {
        private AggtBlocks _blocksInfo;

        public AggtBlocks AggtBlocks
        {
            get
            {
                return this._blocksInfo;
            }
        }

        public virtual void OnArraging(AggtBlocks blocksInfo)
        {
            this._blocksInfo = blocksInfo;
        }

        public virtual void TryArrangeMain(ControlResult control) { }

        public virtual void TryArrangeCommandsContainer(ControlResult control) { }

        public virtual void TryArrangeNavigate(ControlResult control) { }

        public virtual void TryArrangeCondition(ControlResult control) { }

        public virtual void TryArrangeResult(ControlResult control) { }

        public virtual void TryArrangeList(ControlResult control) { }

        public virtual void TryArrangeDetail(ControlResult control) { }

        /// <summary>
        /// 重写此方法来实现新的孩子布局。
        /// 
        /// 默认情况下，尝试使用 ChildrenTab 返回的 TabControl 来安排所有孩子对象。
        /// </summary>
        /// <param name="children"></param>
        public virtual void TryArrangeChildren(IList<Region> children)
        {
            var childrenTab = this.ChildrenTab;
            childrenTab.Style = OEAStyles.TabControlHeaderHide;

            if (childrenTab != null)
            {
                if (children.Count > 0)
                {
                    foreach (var child in children)
                    {
                        var tabItem = CreateATabItem(child);
                        childrenTab.Items.Add(tabItem);
                    }

                    var parentView = children[0].ControlResult.MainView.Parent;
                    ViewAdapter.AdaptView(parentView, childrenTab);
                }
                else
                {
                    childrenTab.RemoveFromParent(false);
                }
            }
        }

        /// <summary>
        /// 如果使用默认的孩子布局方法，则需要实现此属性返回一个可放置孩子控件的 TabControl
        /// </summary>
        protected virtual TabControl ChildrenTab
        {
            get { return null; }
        }

        internal TabControl TryGetChildrenTab()
        {
            return this.ChildrenTab;
        }

        /// <summary>
        /// 默认约定的区域已经放置完成后触发。
        /// </summary>
        public void OnArranged()
        {
            this.OnArrangedCore();
        }

        protected virtual void OnArrangedCore() { }

        private static TabItem CreateATabItem(Region child)
        {
            var label = child.ChildrenLabel ?? child.ControlResult.MainView.Meta.Label;

            var tabHeader = new Label()
            {
                Content = label
            };

            tabHeader.MouseDoubleClick += (s, e) =>
            {
                CommandRepository.TryExecuteCommand(typeof(MaxShowViewCommand), child.ControlResult.MainView);
            };

            var tabItem = new TabItem()
            {
                Header = tabHeader,
                Content = child.ControlResult.Control
            };

            AutomationProperties.SetName(tabItem, label);

            WPFMeta.SetObjectView(tabItem, child.ControlResult.MainView);
            ViewAdapter.AdaptView(child.ControlResult.MainView, tabItem);

            return tabItem;
        }
    }
}
