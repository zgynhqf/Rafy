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
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Command;

namespace Rafy.RBAC.WPF
{
    /// <summary>
    /// 清空日志
    /// </summary>
    [Command(ImageName = "pack://application:,,,/Rafy.WPF;component/Images/Cleanup.bmp", Label = "清空日志", ToolTip = "清空日志")]
    public class ClearAuditLogCommand : ViewCommand
    {
        public override bool CanExecute(LogicalView view)
        {
            var list = view.Data as IList;
            return list != null && list.Count > 0;
        }

        public override void Execute(LogicalView view)
        {
            var result = App.MessageBox.Show("确定清空所有日志？".Translate(), "请确认".Translate(), MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            //清空
            ServiceFactory.Create<ClearLogService>().Invoke();

            //刷新数据
            if (view.DataLoader.AnyLoaded)
            {
                view.DataLoader.ReloadDataAsync();
            }
        }
    }
}
