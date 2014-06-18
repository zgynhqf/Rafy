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
using Rafy.MetaModel.Attributes;
using Rafy.WPF.Command;
using Rafy.WPF;
using Rafy.Domain;

namespace Rafy.RBAC.WPF
{
    [Command(Label = "选择用户")]
    class ChooseUserCommand : LookupSelectAddCommand
    {
        public ChooseUserCommand()
        {
            this.TargetEntityType = typeof(User);
            this.RefProperty = OrgPositionUser.UserProperty;
        }

        protected override WindowButton PopupSelectionWindow(ControlResult ui)
        {
            return App.Windows.ShowDialog(ui.Control, w =>
            {
                w.Title = this.Meta.Label.Translate();
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
            this.RefProperty = OrgPosition.PositionProperty;
        }

        protected override WindowButton PopupSelectionWindow(ControlResult ui)
        {
            return App.Windows.ShowDialog(ui.Control, w =>
            {
                w.Title = this.Meta.Label.Translate();
                w.Width = 600;
                w.Height = 300;
            });
        }
    }
}