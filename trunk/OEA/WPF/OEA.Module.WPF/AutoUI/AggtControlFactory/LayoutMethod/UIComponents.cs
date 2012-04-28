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
using System.Collections;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 封装 RegionContainer 以提供更易用的接口。
    /// 
    /// 默认提供了：
    /// * 传统组件的查找。
    /// * 使用 TabControl 摆放子区域的逻辑。
    /// </summary>
    public class UIComponents : IEnumerable<Region>
    {
        private RegionContainer _regions;

        internal UIComponents(RegionContainer regions)
        {
            this._regions = regions;
        }

        /// <summary>
        /// 描述这些组件如何布局的元数据
        /// </summary>
        public LayoutMeta LayoutMeta
        {
            get { return this._regions.BlocksInfo.Layout; }
        }

        /// <summary>
        /// 尝试使用名字查找控件。
        /// 未找到，则返回null。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ControlResult FindControl(string name)
        {
            return this._regions.FindControl(name);
        }

        /// <summary>
        /// 尝试使用名字查找一组控件。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<ControlResult> FindControls(string name)
        {
            return this.Where(r => r.Name == name).Select(r => r.ControlResult);
        }

        #region 传统组件

        public ControlResult Main
        {
            get { return this.FindControl(TraditionalRegions.Main); }
        }

        public ControlResult CommandsContainer
        {
            get { return this.FindControl(TraditionalRegions.CommandsContainer); }
        }

        public ControlResult Navigation
        {
            get { return this.FindControl(NavigationBlock.Type); }
        }

        public ControlResult Condition
        {
            get { return this.FindControl(ConditionBlock.Type); }
        }

        public ControlResult Result
        {
            get { return this.FindControl(QueryObjectView.ResultSurrounderType); }
        }

        public IList<Region> Children
        {
            get { return this._regions.GetChildrenRegions().ToList(); }
        }

        #endregion

        #region ArrangeChildrenByTabControl

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

        #endregion

        #region IEnumerable Members

        IEnumerator<Region> IEnumerable<Region>.GetEnumerator()
        {
            return this._regions.Regions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._regions.Regions.GetEnumerator();
        }

        #endregion
    }
}