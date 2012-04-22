/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120326
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120326
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel.Attributes;
using OEA.WPF.Command;
using OEA.Module.WPF;
using OEA.Library;
using OEA.RBAC;

namespace RBAC
{
    [Command(Label = "选择用户")]
    class ChooseUserCommand : LookupSelectAddCommand
    {
        public ChooseUserCommand()
        {
            this.TargetEntityType = typeof(User);
            this.RefProperty = OrgPositionUser.UserRefProperty;
        }

        protected override WindowButton PopupSelectionWindow(ControlResult ui)
        {
            return App.Windows.ShowDialog(ui.Control, w =>
            {
                w.Title = this.CommandInfo.Label;
                w.Width = 600;
                w.Height = 300;
            });
        }
    }

    [Command(Label = "选择岗位")]
    class ChoosePositionCommand : LookupSelectAddCommand
    {
        public ChoosePositionCommand()
        {
            this.TargetEntityType = typeof(Position);
            this.RefProperty = OrgPosition.PositionRefProperty;
        }

        protected override WindowButton PopupSelectionWindow(ControlResult ui)
        {
            return App.Windows.ShowDialog(ui.Control, w =>
            {
                w.Title = this.CommandInfo.Label;
                w.Width = 600;
                w.Height = 300;
            });
        }
    }
}