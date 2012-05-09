/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2010
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2010
 * 
*******************************************************/

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
using OEA.RBAC;
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
            var result = App.MessageBox.Show("确定清空所有日志？", "请确认", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            //清空
            new ClearLogService().Invoke();

            //刷新数据
            view.DataLoader.ReloadDataAsync();
        }
    }
}
