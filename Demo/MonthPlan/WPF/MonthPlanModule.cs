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
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Controls;
using Rafy.WPF.Command;

namespace MP.WPF
{
    public class MonthPlanModule : UITemplate
    {
        protected override AggtBlocks DefineBlocks()
        {
            return new AggtBlocks
            {
                MainBlock = new Block(typeof(MonthPlan)),
                Layout = new LayoutMeta()
                {
                    IsLayoutChildrenHorizonal = true,
                    ParentChildProportion = new ParentChildProportion(3, 7)
                },
                Children = 
                {
                    new AggtBlocks
                    {
                        MainBlock = new ChildBlock(MonthPlan.TaskListProperty),
                        Layout = new LayoutMeta()
                        {
                            ParentChildProportion = new ParentChildProportion(6, 4)
                        },
                        Children = 
                        {
                            new ChildBlock(TaskOrCategory.WeekCompletionListProperty),
                        }
                    },
                    new ChildBlock(MonthPlan.WeekNoteListProperty)
                }
            };
        }

        private ListLogicalView _planView;

        private ListLogicalView _taskView;

        private ListLogicalView _weekCompletionView;

        protected override void OnUIGenerated(ControlResult ui)
        {
            base.OnUIGenerated(ui);

            this._planView = ui.MainView as ListLogicalView;
            this._taskView = this._planView.GetChildView(typeof(TaskOrCategory)) as ListLogicalView;
            this._weekCompletionView = this._taskView.GetChildView(typeof(WeekCompletion)) as ListLogicalView;
            var weekNoteView = this._planView.GetChildView(typeof(WeekNote)) as ListLogicalView;

            MPHelper.ModifyRowStyle(this._taskView, "TaskViewRowStyle");
            MPHelper.ModifyRowStyle(this._weekCompletionView, "CurrentWeekRowStyle");
            MPHelper.ModifyRowStyle(weekNoteView, "CurrentWeekRowStyle");

            this.ChangeTaskCompletionVisibility();

            this.AddWeekNotesOnPlan();
        }

        /// <summary>
        /// 只在选中 Task 而非 Category 时，才显示完成页签。
        /// </summary>
        private void ChangeTaskCompletionVisibility()
        {
            this._weekCompletionView.EnableResetVisibility = false;

            this._taskView.CurrentChanged += (o, e) =>
            {
                var task = this._taskView.Current as TaskOrCategory;
                this._weekCompletionView.IsVisible = task != null && task.IsTaskRO;
            };
        }

        /// <summary>
        /// 添加月内每个周的说明
        /// </summary>
        private void AddWeekNotesOnPlan()
        {
            var cmd = this._planView.Commands.Find<PopupAddCommand>();
            cmd.DataCloned += (o, e) =>
            {
                var plan = e.NewEntity as MonthPlan;
                var weeksCount = plan.WeeksCountRO;
                for (int i = 0; i < weeksCount; i++)
                {
                    plan.WeekNoteList.Add(new WeekNote
                    {
                        Index = i + 1
                    });
                }
            };
        }
    }
}
