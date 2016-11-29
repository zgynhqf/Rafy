using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Command;

namespace Rafy.RBAC.Old.WPF
{
    [Command(Label = "统计登录")]
    public class LoginLogStatisticCommand : ListViewCommand
    {
        public override void Execute(ListLogicalView view)
        {
            var list = RF.Find<UserLoginLog>().GetAll()
                .OfType<UserLoginLog>()
                .ToList();

            var control = new LoginLogStatistic(list);
            App.Windows.ShowDialog(control, w =>
            {
                w.Title = "登录时间统计".Translate();
                w.MinHeight = 300;
                w.SizeToContent = SizeToContent.WidthAndHeight;
            });
        }
    }
}