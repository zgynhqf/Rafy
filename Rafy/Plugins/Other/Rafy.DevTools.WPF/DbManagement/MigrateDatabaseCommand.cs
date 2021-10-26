/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130107 09:32
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130107 09:32
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Controls;
using Rafy.Domain.ORM.DbMigration;
using Rafy.Domain.ORM.DbMigration.Presistence;
using Rafy.WPF.Command;

namespace Rafy.DevTools.DbManagement
{
    [Command(Label = "数据库升级", ToolTip = "生成或升级指定的数据库", GroupType = CommandGroupType.Business)]
    public class MigrateDatabaseCommand : ViewCommand
    {
        public override void Execute(LogicalView view)
        {
            //设置查询时间为从当前时间起。
            var queryView = view.ConditionQueryView;
            if (queryView != null)
            {
                var criteria = queryView.Current as DbMigrationHistoryQueryCriteria;
                criteria.StartTime = DateTime.Now;
            }

            //升级数据库。
            ClientMigrationHelper.MigrateOnClient();

            //重新加载数据。
            if (view.DataLoader.AnyLoaded) { view.DataLoader.ReloadDataAsync(); }
        }
    }
}