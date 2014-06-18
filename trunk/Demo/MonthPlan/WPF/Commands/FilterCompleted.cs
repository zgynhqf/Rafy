/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121107 11:08
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121107 11:08
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.Domain;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Command;

namespace MP.WPF.Commands
{
    [Command(Label = "过滤完成项", GroupType = CommandGroupType.View)]
    public class FilterCompleted : ListViewCommand
    {
        private bool _initialized = false;

        public override bool CanExecute(ListLogicalView view)
        {
            bool canExecute = false;

            if (view.Data != null)
            {
                foreach (var item in view.Data)
                {
                    if (!this.CanShow(item))
                    {
                        canExecute = true;
                        break;
                    }
                }
            }

            //第一次运行时，执行过滤。
            if (canExecute && !this._initialized)
            {
                this.Execute(view);
                this._initialized = true;
            }

            return canExecute;
        }

        public override void Execute(ListLogicalView view)
        {
            if (view.Filter != null)
            {
                view.Filter = null;
                this.Label = this.Meta.Label;
            }
            else
            {
                view.Filter = this.CanShow;
                this.Label = "显示全部";
            }
        }

        private bool CanShow(Entity entity)
        {
            var weekSummary = entity as WeekSummary;
            if (weekSummary != null) return this.CanShow(weekSummary);

            var task = entity as TaskOrCategory;
            if (task != null) return this.CanShow(task);

            throw new NotSupportedException();
        }

        private bool CanShow(WeekSummary entity)
        {
            return !entity.IsCompletedRO;
        }

        private bool CanShow(TaskOrCategory entity)
        {
            return !entity.IsTaskCompletedRO;
        }
    }
}
