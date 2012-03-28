using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace OEA.UIA.TestCaseDemo
{
    public class 专业字典Demo : TestCase
    {
        protected override void RunOverride()
        {
            //TestByAutomationElementAPI();
            //return;

            var module = OpenModule("基础字典.专业字典");
            进入(module, m =>
            {
                点击按钮("添加");
                列表("专业字典").当前行().属性编辑器("专业名称").输入("AutoTest");
                点击按钮("保存");

                点击按钮("复制添加");
                点击按钮("保存");
                按住Ctrl();
                列表().选择行(11);
                释放Ctrl();
                点击按钮("删除");
                窗口("确认删除").点击按钮("Yes");
                点击按钮("保存");

                点击按钮("添加");
                点击按钮("取消");
                点击按钮("刷新");
            });
        }

        private static void TestByAutomationElementAPI()
        {
            等待(1);

            var window = new TreeWalker(
                    new AndCondition
                    (
                    new PropertyCondition(AutomationElement.NameProperty, "指标管理系统"),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window)
                    )
                ).GetFirstChild(AutomationElement.RootElement);
            var page = window.FindFirst(TreeScope.Subtree,
                new AndCondition
                (
                    new PropertyCondition(AutomationElement.NameProperty, "专业字典"),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem)
                ));
            var button = page.FindFirst(TreeScope.Subtree,
                new AndCondition
                (
                    new PropertyCondition(AutomationElement.NameProperty, "添加"),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button)
                ));

            var p1 = button.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
            p1.Invoke();

            var treeGrid = page.FindFirst(TreeScope.Subtree,
                new AndCondition
                (
                    new PropertyCondition(AutomationElement.NameProperty, "专业字典"),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Tree)
                ));

            var siPattern = treeGrid.GetCurrentPattern(SelectionPattern.Pattern) as SelectionPattern;
            var rowElement = siPattern.Current.GetSelection()[0];

            var cellElement = rowElement.FindFirst(TreeScope.Subtree,
                new AndCondition
                (
                    new PropertyCondition(AutomationElement.NameProperty, "专业名称"),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom)
                ));

            var value = cellElement.GetCurrentPropertyValue(AutomationElement.IsContentElementProperty);
            value = cellElement.GetCurrentPropertyValue(AutomationElement.IsContentElementProperty);
            var p2 = cellElement.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
            p2.Invoke();
            //var wpfCell = Microsoft.VisualStudio.TestTools.UITesting.UITestControlFactory.FromNativeElement(cell, "UIA") as WpfControl;
            //wpfCell.点击();

            var editingCellElement = rowElement.FindFirst(TreeScope.Subtree, new PropertyCondition(AutomationElement.NameProperty, "专业名称"));

            var vp = editingCellElement.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
            vp.SetValue("DDDDDDDDDDDDDDDDDDDDD");

            //var edit = Microsoft.VisualStudio.TestTools.UITesting.UITestControlFactory.FromNativeElement(editingCellElement, "UIA") as WpfEdit;
            //edit.Text = "ddddddddddd";

            //var patterns = editingCell.GetSupportedPatterns();

            return;
        }
    }
}
