/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121102 16:33
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121102 16:33
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Controls;
using Rafy.WPF.Command;

namespace MP.WPF.Commands
{
    public abstract class SetPlanStatus : ListViewCommand
    {
        protected MonthPlanStatus Status;

        public override bool CanExecute(ListLogicalView view)
        {
            var plan = view.Current as MonthPlan;
            return plan != null && !plan.IsDirty;
        }

        public override void Execute(ListLogicalView view)
        {
            var plan = view.Current as MonthPlan;

            plan.PlanStatus = Status;
            RF.Save(plan);

            App.MessageBox.Show("状态设置成功。");
        }
    }

    [Command(Label = "回到计划中", GroupType = CommandGroupType.System)]
    public class SetPlanStatus_Planning : SetPlanStatus
    {
        public SetPlanStatus_Planning()
        {
            this.Status = MonthPlanStatus.Planning;
        }
    }

    [Command(Label = "回到总结中", GroupType = CommandGroupType.System)]
    public class SetPlanStatus_Summarizing : SetPlanStatus
    {
        public SetPlanStatus_Summarizing()
        {
            this.Status = MonthPlanStatus.Summarizing;
        }
    }

    [Command(Label = "完成计划", GroupType = CommandGroupType.Business)]
    public class LockPlan : ListViewCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            var plan = view.Current as MonthPlan;
            return plan != null && !plan.IsDirty && plan.PlanStatus == MonthPlanStatus.Planning;
        }

        public override void Execute(ListLogicalView view)
        {
            var plan = view.Current as MonthPlan;

            var error = CheckPlan(plan);
            if (!string.IsNullOrEmpty(error))
            {
                App.MessageBox.Show(error, "计划未完成", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var btn = App.MessageBox.Show("完成后不可修改计划，是否继续？", MessageBoxButton.YesNo);
            if (btn == MessageBoxResult.Yes)
            {
                plan.CreateTime = DateTime.Now;
                plan.PlanStatus = MonthPlanStatus.Summarizing;
                RF.Save(plan);

                App.MessageBox.Show("计划已完成，计划状态锁定，可随时总结已完成指标值。");
            }
        }

        private static string CheckPlan(MonthPlan plan)
        {
            var tasks = plan.TaskOrCategoryList;
            var allPercent = tasks.Cast<TaskOrCategory>().Sum(c => c.MonthPercent);
            if (allPercent != 100)
            {
                return "类别百分比的和应该是 100，目前总和为：" + allPercent;
            }

            foreach (TaskOrCategory category in tasks)
            {
                if (category.IsCategoryRO)
                {
                    if (category.TreeChildren.Count == 0)
                    {
                        return "每个类别下最少必须有一个任务，保存失败。";
                    }
                }
            }

            return null;
        }
    }

    [Command(Label = "完成本月总结", GroupType = CommandGroupType.Business)]
    public class CompletePlan : ListViewCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            var plan = view.Current as MonthPlan;
            return plan != null && !plan.IsDirty && plan.PlanStatus == MonthPlanStatus.Summarizing;
        }

        public override void Execute(ListLogicalView view)
        {
            var plan = view.Current as MonthPlan;
            var btn = App.MessageBox.Show(string.Format("目前得分：{0}，提交总结后不可再修改总结及得分，是否继续？", plan.FinalScore), MessageBoxButton.YesNo);
            if (btn == MessageBoxResult.Yes)
            {
                plan.CompleteTime = DateTime.Now;
                plan.PlanStatus = MonthPlanStatus.Completed;
                RF.Save(plan);

                App.MessageBox.Show("计划总结已完成，状态锁定！");
            }
        }
    }

    [Command(Label = "本周执行", ToolTip = "填写本周执行情况", Gestures = "F1", GroupType = CommandGroupType.Business)]
    public class ShowWeekSummary : ListViewCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            return view.Data != null;
        }

        public override void Execute(ListLogicalView view)
        {
            var plan = LocateCurrentSummarizingMonth(view);
            if (plan == null)
            {
                App.MessageBox.Show("没有找到本月正在进行中的计划。", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }

            var block = new Block(typeof(WeekSummary));

            var ui = AutoUI.AggtUIFactory.GenerateControl(block);

            var weekView = ui.MainView as ListLogicalView;

            MPHelper.ModifyRowStyle(weekView, "WeekSummaryRowStyle");

            SetData(weekView, plan);

            weekView.ExpandAll();

            var btn = App.Windows.ShowDialog(ui.Control, win =>
            {
                win.Title = string.Format("第 {0} 周计划与总结", TimeHelper.CurrentWeekIndex + 1);
                win.Buttons = ViewDialogButtons.YesNo;
            });

            if (btn == WindowButton.Yes)
            {
                ReadData(weekView, plan);
            }
        }

        /// <summary>
        /// 找到并定位到正在进行中的月
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        private static MonthPlan LocateCurrentSummarizingMonth(ListLogicalView view)
        {
            var today = DateTime.Today;

            foreach (MonthPlan item in view.Data)
            {
                if (item.PlanStatus == MonthPlanStatus.Summarizing)
                {
                    var m = item.Month;
                    if (m.Year == today.Year && m.Month == today.Month)
                    {
                        view.Current = item;
                        return item;
                    }
                }
            }

            return null;
        }

        private static void SetData(ListLogicalView weekView, MonthPlan plan)
        {
            var weekIndex = TimeHelper.CurrentWeekIndex;

            var weekSummaryList = new WeekSummaryList();
            weekSummaryList.AutoTreeIndexEnabled = false;

            //把 plan.TaskOrCategoryList 中的数据复制到 WeekSummaryList 中。
            foreach (TaskOrCategory category in plan.TaskOrCategoryList)
            {
                var categoryWeek = new WeekSummary();
                categoryWeek.IsCategory = true;
                categoryWeek.TreeIndex = category.TreeIndex;
                categoryWeek.Name = category.Name;
                categoryWeek.Content = category.Content;
                categoryWeek.Id = category.Id;
                categoryWeek.TreePId = category.TreePId;
                weekSummaryList.Add(categoryWeek);

                foreach (TaskOrCategory task in category.TreeChildren)
                {
                    var taskWeek = new WeekSummary();
                    taskWeek.IsTask = true;
                    taskWeek.TreeIndex = task.TreeIndex;
                    taskWeek.Name = task.Name;
                    taskWeek.Content = task.Content;
                    taskWeek.ObjectiveName = task.ObjectiveName;
                    taskWeek.Id = task.Id;
                    taskWeek.TreePId = task.TreePId;
                    taskWeek.MonthObjectiveNum = task.ObjectiveNum;
                    taskWeek.MonthCompletedNum = task.SumCompletedRO;

                    var weekCompletion = task.WeekCompletionList[weekIndex] as WeekCompletion;
                    taskWeek.WeekCompletion = weekCompletion;
                    taskWeek.ObjectiveNum = weekCompletion.ObjectiveNum;
                    taskWeek.NumCompleted = weekCompletion.NumCompleted;
                    taskWeek.WeekSummaryNote = weekCompletion.Note;

                    categoryWeek.TreeChildren.Add(taskWeek);
                    weekSummaryList.Add(taskWeek);
                }
            }

            weekView.Data = weekSummaryList;
        }

        private static void ReadData(ListLogicalView weekView, MonthPlan plan)
        {
            foreach (WeekSummary week in weekView.Data)
            {
                if (week.IsTask)
                {
                    var completion = week.WeekCompletion;
                    completion.NumCompleted = week.NumCompleted;
                    completion.Note = week.WeekSummaryNote;
                }
            }
        }
    }

    [Command(Label = "删除", ImageName = "Delete.bmp", GroupType = CommandGroupType.Edit)]
    public class DeletePlan : DeleteListObjectCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            return base.CanExecute(view) && (view.Current as MonthPlan).PlanStatus == MonthPlanStatus.Planning;
        }
    }

    [Command(Label = "保存", ImageName = "Save.bmp", Gestures = "Ctrl+S", GroupType = CommandGroupType.Edit)]
    public class SaveMonthPlan : SaveListCommand { }

    [Command(Label = "展开", GroupType = CommandGroupType.View)]
    public class ExpandTask : ListViewCommand
    {
        private bool _initialized = false;

        private ListLogicalView _view;

        protected bool _expanded = false;

        public override bool CanExecute(ListLogicalView view)
        {
            if (!this._initialized)
            {
                this._view = view;
                view.DataChanged += this.OnDataChanged;

                //默认处于展开状态。
                this.SetExpanded(true);

                this._initialized = true;
            }

            return view.Data != null && view.Data.Count > 0;
        }

        public override void Execute(ListLogicalView view)
        {
            this.SetExpanded(!this._expanded);
        }

        private void OnDataChanged(object sender, EventArgs e)
        {
            this.SetExpanded(true);
        }

        private void SetExpanded(bool value)
        {
            var grid = this._view.Control as TreeGrid;
            if (grid != null)
            {
                this._expanded = value;

                if (value)
                {
                    grid.ExpandAll();
                    this.Label = "折叠";
                }
                else
                {
                    grid.CollapseAll();
                    this.Label = "展开";
                }
            }
        }
    }

    [Command(Label = "添加类别", ToolTip = "添加一个类别", Gestures = "F5", GroupType = CommandGroupType.Edit)]
    public class AddCategory : ListViewCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            return view.CanAddItem() && CommandsHelper.IsPlanning(view);
        }

        public override void Execute(ListLogicalView view)
        {
            var category = view.CreateNewItem() as TaskOrCategory;

            category.MonthPercent = 0;
            category.MonthScore = 0;

            view.Data.Add(category);

            view.RefreshControl();

            view.Current = category;

            CommandsHelper.EditCurrent(view);
        }
    }

    [Command(Label = "添加任务", ToolTip = "添加一个任务", Gestures = "F6", GroupType = CommandGroupType.Edit)]
    public class AddTask : ListViewCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            return view.CanAddItem() && view.Current != null && CommandsHelper.IsPlanning(view);
        }

        public override void Execute(ListLogicalView view)
        {
            var category = view.Current as TaskOrCategory;
            if (category.IsTaskRO)
            {
                category = category.TreeParent as TaskOrCategory;
            }

            var task = view.CreateNewItem() as TaskOrCategory;

            var weeksCount = task.MonthPlan.WeeksCountRO;
            for (int i = 0; i < weeksCount; i++)
            {
                task.WeekCompletionList.Add(new WeekCompletion
                {
                    Index = i + 1
                });
            }

            task.WeightInCategory = 1;
            task.Score = 0;
            task.ObjectiveNum = 1;//放在添加 WeekCompletionList 之后

            category.TreeChildren.Add(task);

            view.RefreshControl();

            view.Current = task;

            CommandsHelper.EditCurrent(view);
        }
    }

    [Command(Label = "删除", GroupType = CommandGroupType.Edit)]
    public class DeleteTask : DeleteListObjectCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            return base.CanExecute(view) && CommandsHelper.IsPlanning(view);
        }
    }

    [Command(Label = "上移", GroupType = CommandGroupType.Edit)]
    public class MoveUpTask : MoveUpCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            return base.CanExecute(view) && CommandsHelper.IsPlanning(view);
        }
    }

    [Command(Label = "下移", GroupType = CommandGroupType.Edit)]
    public class MoveDownTask : MoveDownCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            return base.CanExecute(view) && CommandsHelper.IsPlanning(view);
        }
    }

    public static class CommandsHelper
    {
        public static bool IsPlanning(ListLogicalView taskView)
        {
            var plan = taskView.Parent.Current as MonthPlan;
            return plan != null && plan.PlanStatus == MonthPlanStatus.Planning;
        }

        public static bool IsSummarizing(ListLogicalView taskView)
        {
            var plan = taskView.Parent.Current as MonthPlan;
            return plan != null && plan.PlanStatus == MonthPlanStatus.Summarizing;
        }

        public static void EditCurrent(ListLogicalView view)
        {
            //开始编辑该行。
            var grid = view.Control as RafyTreeGrid;
            WPFHelper.InvokeUntil(() =>
            {
                var row = grid.FindRow(grid.SelectedItem);
                if (row != null)
                {
                    var column = grid.FindColumnByProperty(TaskOrCategory.NameProperty);
                    var cell = row.FindCell(column);

                    grid.TryEditRow(row, cell);

                    return true;
                }

                return false;
            });
        }
    }

    [Command(Label = "重新指导", ToolTip = "把之前未完成的目标，都分配到本周及后几周。", GroupType = CommandGroupType.Business)]
    public class ResignWeekSubjectiveNum : ListViewCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            var task = view.Parent.Current as TaskOrCategory;
            return task != null && task.MonthPlan.PlanStatus == MonthPlanStatus.Summarizing;
        }

        public override void Execute(ListLogicalView view)
        {
            var task = view.Parent.Current as TaskOrCategory;
            var weekList = task.WeekCompletionList;
            var currentIndex = TimeHelper.CurrentWeekIndex;

            var sum = 0;
            for (int i = 0, c = weekList.Count; i < c; i++)
            {
                var week = weekList[i] as WeekCompletion;

                //本周前的所有周，把它们所有未完成的指标，都收集起来，然后平均分配到后面的几个周。
                if (i < currentIndex)
                {
                    var notCompleted = week.ObjectiveNum - week.NumCompleted;
                    if (notCompleted > 0)
                    {
                        sum += notCompleted;
                        week.ObjectiveNum = week.NumCompleted;
                    }
                }
                else
                {
                    if (sum == 0) { break; }

                    week.ObjectiveNum++;
                    sum--;

                    //最后一周，还没有分配完，则重新从本周开始分配。
                    if (i == c - 1 && sum > 0)
                    {
                        i = currentIndex - 1;
                    }
                }
            }
        }
    }
}