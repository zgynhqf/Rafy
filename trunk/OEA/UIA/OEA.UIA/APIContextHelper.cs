/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111201
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111201
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using OEA.UIA.Utils;
using WinUIA = System.Windows.Automation;
using System.Threading;

namespace OEA.UIA
{
    /// <summary>
    /// 流畅接口的扩展方法类
    /// </summary>
    public static class APIContextHelper
    {
        #region WpfControl 动作

        public static WpfControl 单击(this WpfControl control)
        {
            control.EnsureClickable();
            control.WaitForControlEnabled();
            Mouse.Click(control);

            return control;
        }

        public static WpfControl 双击(this WpfControl control)
        {
            control.EnsureClickable();
            control.WaitForControlEnabled();
            Mouse.DoubleClick(control);

            return control;
        }

        public static WpfEdit 输入(this WpfEdit control, string value)
        {
            control.Text = value;
            return control;
        }

        public static WpfDatePicker 输入(this WpfDatePicker control, string value)
        {
            control.DateAsString = value;
            return control;
        }

        /// <summary>
        /// 对下拉列表使用
        /// </summary>
        /// <param name="control"></param>
        /// <param name="allRowsExpression">
        /// 接受如下格式：
        /// A/B;A/B/C;!A/C;!A
        /// 
        /// 分隔符：使用 / 作为分隔符，如果数据中有此字符，则可以使用 $ 来分隔。
        /// !表示取消该行
        /// </param>
        /// <returns></returns>
        public static WpfComboListControl 输入(this WpfComboListControl control, string allRowsExpression)
        {
            var comboBox = control.Control;

            //ComboListControlAutomationPeer 中会把 TreeGrid 放在最后一个。
            var children = comboBox.GetChildren();
            var treeGrid = children[children.Count - 1] as WpfCustom;
            if (treeGrid == null) throw new UIAutomationException("没有找到下拉树型控件。");

            comboBox.Expanded = true;

            treeGrid.树形列表().选择行(allRowsExpression);

            comboBox.Expanded = false;

            //var popupWin = APIContext.弹出窗口();
            //var treeGrid = popupWin.Find<WpfCustom>("编辑控件");
            //treeGrid.DrawHighlight();
            //var tree = treeGrid.树形列表();
            //tree.DrawHighlight();
            //tree.选择行(allRowsExpression);

            return control;
        }

        public static WpfComboBox 输入(this WpfComboBox control, string value)
        {
            control.SelectedItem = value;
            return control;
        }

        public static WpfButton 点击按钮(this WpfControl context, string title)
        {
            Logger.LogLine("点击按钮：" + title);

            var btn = context.按钮(title);
            btn.单击();

            return btn;
        }

        #endregion

        #region WpfControl - 树型控件查找

        public static WpfTree 列表(this WpfControl parent, string title = null)
        {
            return 树形列表(parent, title);
        }

        public static WpfTree 树形列表(this WpfControl context, string title = null)
        {
            return context.Find<WpfTree>(title);
        }

        public static WpfTreeItem 当前行(this WpfTree tree)
        {
            foreach (WpfTreeItem row in tree.Nodes)
            {
                if (row.Selected) { return row; }
            }

            throw new UIAutomationException("当前行没有返回值：Grid 没有选择行");
        }

        public static WpfTreeItem 选择行(this WpfTree tree, int rowIndex)
        {
            var item = tree.Find<WpfTreeItem>();
            item.SearchProperties[WpfTreeItem.PropertyNames.Instance] = (rowIndex + 1).ToString();
            item.SearchConfigurations.Add(SearchConfiguration.ExpandWhileSearching);

            item.Selected = true;
            return item;
        }

        /// <summary>
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="allRowsExpression">
        /// 接受如下格式：
        /// A/B;A/B/C;!A/C;!A
        /// 
        /// !表示取消该行
        /// 多行使用 ; 分隔。
        /// 行内层级使用 / 作为分隔符，如果数据中有此字符，则可以使用 $ 来分隔。
        /// </param>
        /// <returns></returns>
        public static WpfTreeItem 选择行(this WpfTree tree, string allRowsExpression)
        {
            var rowPathes = allRowsExpression.SplitBy(";");

            //由于 WinUIA 的速度太快，需要一定的时间来等待控件生成。
            Thread.Sleep(100);

            WpfTreeItem row = null;

            foreach (var rowPath in rowPathes)
            {
                if (rowPath[0] == '!')
                {
                    row = tree.行(rowPath.Substring(1));
                    var pattern = (row.NativeElement as WinUIA.AutomationElement)
                        .GetCurrentPattern(WinUIA.SelectionItemPattern.Pattern) as WinUIA.SelectionItemPattern;
                    pattern.RemoveFromSelection();

                    //WpfTreeItem 不支持
                    //row.Selected = false;
                }
                else
                {
                    row = tree.行(rowPath);
                    row.Selected = true;
                }
            }

            return row;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="path">使用 / 作为分隔符，如果数据中有此字符，则可以使用 $ 来分隔。</param>
        /// <returns></returns>
        public static WpfTreeItem 行(this WpfTree tree, string path)
        {
            string[] pathes = null;
            if (path.Contains("$") && path.Contains("/"))
            {
                pathes = path.SplitBy("$");
            }
            else
            {
                pathes = path.SplitBy("/");
            }

            if (pathes.Length > 0)
            {
                tree.Find();
                WpfControl parent = tree;
                for (int i = 0, c = pathes.Length; i < c; i++)
                {
                    var title = pathes[i];

                    //方案一
                    //var nodes = GetNodes(parent);
                    //for (int j = 0, c2 = nodes.Count; j < c2; j++)
                    //{
                    //    var node = nodes[j] as WpfTreeItem;
                    //    if (node.Name == title)
                    //    {
                    //        if (i < c - 1) { node.Expanded = true; }
                    //        parent = node;
                    //        break;
                    //    }
                    //}

                    //方案二
                    //var node = parent.Find<WpfTreeItem>(title);
                    //node.SearchConfigurations.Add(SearchConfiguration.ExpandWhileSearching);
                    //node.Find();
                    //if (i < c - 1) { node.Expanded = true; }
                    //parent = node;

                    //方案三
                    var node = parent.Find<WpfTreeItem>(title);
                    if (i < c - 1)
                    {
                        //由于 WinUIA 的速度太快，需要一定的时间来等待控件生成。
                        Thread.Sleep(100);
                        var pattern = (node.NativeElement as WinUIA.AutomationElement)
                            .GetCurrentPattern(WinUIA.ExpandCollapsePattern.Pattern) as WinUIA.ExpandCollapsePattern;
                        pattern.Expand();
                    }
                    parent = node;
                }

                if (parent is WpfTreeItem) { return parent as WpfTreeItem; }
            }

            throw new UIAutomationException("没有找到对应的行：" + path);
        }

        private static UITestControlCollection GetNodes(WpfControl treeItem)
        {
            var item = treeItem as WpfTreeItem;
            if (item != null)
            {
                return item.Nodes;
            }

            return (treeItem as WpfTree).Nodes;
        }

        public static WpfEdit 属性编辑器(this WpfTreeItem row, string title = null)
        {
            return 属性编辑器<WpfEdit>(row, title);
        }

        public static WpfDatePicker 属性编辑器_日期(this WpfTreeItem context, string title = null)
        {
            return 属性编辑器<WpfDatePicker>(context, title);
        }

        public static WpfComboBox 属性编辑器_枚举(this WpfTreeItem context, string title = null)
        {
            return 属性编辑器<WpfComboBox>(context, title);
        }

        public static WpfComboListControl 属性编辑器_下拉列表(this WpfTreeItem context, string title = null)
        {
            return new WpfComboListControl
            {
                Control = 属性编辑器<WpfComboBox>(context, title)
            };
        }

        public static WpfCheckBox 属性编辑器_勾选框(this WpfTreeItem context, string title = null)
        {
            return 属性编辑器<WpfCheckBox>(context, title);
        }

        private static TWpfControl 属性编辑器<TWpfControl>(this WpfTreeItem row, string title = null)
            where TWpfControl : WpfControl
        {
            var rowElement = row.NativeElement as WinUIA.AutomationElement;

            //开启单元格的编辑状态
            var cellElement = rowElement.FindFirst(WinUIA.TreeScope.Subtree,
                new WinUIA.AndCondition
                (
                    new WinUIA.PropertyCondition(WinUIA.AutomationElement.NameProperty, title),
                    new WinUIA.PropertyCondition(WinUIA.AutomationElement.ControlTypeProperty, WinUIA.ControlType.Custom)
                ));
            if (cellElement != null)
            {
                var p2 = cellElement.GetCurrentPattern(WinUIA.InvokePattern.Pattern) as WinUIA.InvokePattern;
                p2.Invoke();
            }

            //由于 WinUIA 的速度太快，需要一定的时间来等待控件生成。
            Thread.Sleep(100);

            //编辑状态打开后，再找到相应的编辑控件。
            var editingCellElement = rowElement.FindFirst(WinUIA.TreeScope.Subtree,
                new WinUIA.PropertyCondition(WinUIA.AutomationElement.NameProperty, "编辑控件")
                );
            var editingCell = UITestControlFactory.FromNativeElement(editingCellElement, "UIA");

            //var cell = UITestControlFactory.FromNativeElement(cellElement, "UIA") as WpfCustom;
            //var cell = row.Find<WpfCustom>(title);
            //cell.Click();

            //cell.SearchProperties[WpfControl.PropertyNames.ClassName] = "Uia.MTTGCell";
            //cell.SearchProperties[WpfControl.PropertyNames.ClassName] = "Uia.AutomatableTextBlock";
            //cell.Find();

            //var editingCell = row.Find<WpfEdit>(title);

            return editingCell as TWpfControl;
        }

        #endregion

        #region WpfControl - 查找

        public static WpfMenuItem 菜单(this WpfControl context, string title = null)
        {
            context = context.Find<WpfMenu>("主菜单");

            var path = title.SplitBy(".");
            if (path.Length > 1)
            {
                for (int i = 0, c = path.Length - 1; i < c; i++)
                {
                    var pageTitle = path[i];
                    var menu = context.菜单(pageTitle);
                    if (!menu.Expanded) menu.Expanded = true;
                    context = menu;
                }
            }

            var mi = context.Find<WpfMenuItem>(path.Last());
            if (!mi.Exists)
            {
                throw new UIAutomationException("没有找到菜单" + title);
            }

            return mi;
        }

        public static WpfMenu 弹出菜单(this WpfControl context)
        {
            return context.Find<WpfMenu>();
        }

        /// <summary>
        /// 右键菜单
        /// </summary>
        /// <param name="context"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static WpfMenuItem 菜单(this WpfMenu context, string title = null)
        {
            return context.Find<WpfMenuItem>(title);
        }

        public static WpfTabPage 页签(this WpfControl context, string title = null)
        {
            var path = title.SplitBy(".");
            context = LocateParentTab(context, path);

            return context.Find<WpfTabPage>(path.Last());
        }

        public static WpfButton 按钮(this WpfControl context, string title = null)
        {
            return context.Find<WpfButton>(title);
        }

        private static WpfControl LocateParentTab(WpfControl context, string[] path)
        {
            if (path.Length > 1)
            {
                for (int i = 0, c = path.Length - 1; i < c; i++)
                {
                    var pageTitle = path[i];
                    context = context.页签(pageTitle);
                }
            }

            return context;
        }

        public static WpfEdit 属性编辑器(this WpfControl context, string title = null)
        {
            return 属性编辑器<WpfEdit>(context, title);
        }

        public static WpfDatePicker 属性编辑器_日期(this WpfControl context, string title = null)
        {
            return 属性编辑器<WpfDatePicker>(context, title);
        }

        public static WpfComboBox 属性编辑器_枚举(this WpfControl context, string title = null)
        {
            return 属性编辑器<WpfComboBox>(context, title);
        }

        public static WpfComboListControl 属性编辑器_下拉列表(this WpfControl context, string title = null)
        {
            return new WpfComboListControl
            {
                Control = 属性编辑器<WpfComboBox>(context, title)
            };
        }

        public static WpfCheckBox 属性编辑器_勾选框(this WpfControl context, string title = null)
        {
            return 属性编辑器<WpfCheckBox>(context, title);
        }

        private static TWpfControl 属性编辑器<TWpfControl>(this WpfControl context, string title = null)
            where TWpfControl : WpfControl, new()
        {
            return context.Find<TWpfControl>(title);
        }

        public static WpfCheckBox 勾选(this WpfCheckBox item)
        {
            item.Checked = true;
            return item;
        }

        public static WpfCheckBox 取消勾选(this WpfCheckBox item)
        {
            item.Checked = true;
            return item;
        }

        public static WpfCheckBox 反选(this WpfCheckBox item)
        {
            item.Checked = !item.Checked;
            return item;
        }

        #endregion

        #region 常用扩展方法

        public static TControl Find<TControl>(this WpfControl context, string title = null)
            where TControl : WpfControl, new()
        {
            return WpfControlFactory.Create<TControl>(title, context);
        }

        public static TControl FindById<TControl>(this WpfControl context, string id)
            where TControl : WpfControl, new()
        {
            return WpfControlFactory.CreateById<TControl>(id, context);
        }

        #endregion
    }

    #region public enum 编辑器类型

    public enum 编辑器类型
    {
        文本编辑框,
        下拉列表,
        下拉树形列表,
        枚举,
        勾选框,
        日期,
    }

    #endregion
}
