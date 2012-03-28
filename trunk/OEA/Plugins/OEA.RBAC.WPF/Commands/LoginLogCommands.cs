using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using OEA.Library;
using OEA.Library.Audit;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.Module.WPF;
using OEA.WPF.Command;
using OEA.RBAC;

namespace RBAC
{
    [Command(Label = "统计登录")]
    public class LoginLogStatisticCommand : ListViewCommand
    {
        public override void Execute(ListObjectView view)
        {
            var list = RF.Create<UserLoginLog>().GetAll()
                .OfType<UserLoginLog>()
                .ToList();

            var control = new LoginLogStatistic(list);
            App.Current.Windows.ShowDialog(control, w =>
            {
                w.Title = "登录时间统计";
                w.MinHeight = 300;
                w.SizeToContent = SizeToContent.WidthAndHeight;
            });
        }
    }
}