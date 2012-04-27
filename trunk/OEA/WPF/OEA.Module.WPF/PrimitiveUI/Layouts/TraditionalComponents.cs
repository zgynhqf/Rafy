/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120416
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120416
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel.View;
using System.Windows.Controls;
using Itenso.Windows.Input;
using OEA.WPF.Command;
using System.Windows.Automation;
using OEA.Module.WPF.Controls;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 尝试布局以下内容：
    /// Main, Toolbar, Navigate, Condition, Result, List, Detail, Children
    /// </summary>
    public class TraditionalComponents
    {
        private RegionContainer _regions;

        public TraditionalComponents(RegionContainer regions)
        {
            this._regions = regions;
        }

        public AggtBlocks AggtBlocks
        {
            get { return this._regions.BlocksInfo; }
        }

        #region 传统组件

        public ControlResult Main
        {
            get { return _regions.TryGetControl(TraditionalRegions.Main); }
        }

        public ControlResult CommandsContainer
        {
            get { return _regions.TryGetControl(TraditionalRegions.CommandsContainer); }
        }

        public ControlResult Navigation
        {
            get { return _regions.TryGetControl(NavigationBlock.Type); }
        }

        public ControlResult Condition
        {
            get { return _regions.TryGetControl(ConditionBlock.Type); }
        }

        public ControlResult Result
        {
            get { return _regions.TryGetControl(QueryObjectView.ResultSurrounderType); }
        }

        public IList<Region> Children
        {
            get { return _regions.GetChildrenRegions().ToList(); }
        }

        #endregion

        /// <summary>
        /// 尝试使用名字查找控件。
        /// 未找到，则返回null。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ControlResult FindControl(string name)
        {
            return this._regions.TryGetControl(name);
        }

        /// <summary>
        /// 外部可以使用这个辅助方法来实现子页签的摆放
        /// </summary>
        /// <param name="childrenTab"></param>
        public void ArrangeChildrenByTabControl(TabControl childrenTab)
        {
            childrenTab.Style = OEAStyles.TabControlHeaderHide;

            var children = this.Children;
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