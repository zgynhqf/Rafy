using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using OEA;
using OEA.Library;
using OEA.Library.Audit;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.Module.WPF;
using OEA.WPF.Command;

namespace RBAC
{
    /// <summary>
    /// 清空日志
    /// </summary>
    [Command(ImageName = "Cleanup.bmp", Label = "清空日志", ToolTip = "清空日志")]
    public class ClearAuditLogCommand : ViewCommand
    {
        public override bool CanExecute(ObjectView view)
        {
            var list = view.Data as IList;
            return list != null && list.Count > 0;
        }

        public override void Execute(ObjectView view)
        {
            var result = App.Current.MessageBox.Show("请确认", "确定清空所有日志？", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            //清空
            new ClearLogService().Invoke();

            //刷新数据
            var refresh = new RefreshCommand();
            refresh.Execute(view as ListObjectView);
        }
    }
}
