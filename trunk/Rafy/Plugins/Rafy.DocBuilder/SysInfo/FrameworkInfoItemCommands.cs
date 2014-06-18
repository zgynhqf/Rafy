/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130110 11:54
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130110 11:54
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Command;

namespace Rafy.DevTools.SysInfo
{
    [Command(Label = "显示服务端状态", ToolTip = "显示服务端当前已经加载数据的状态", GroupType = CommandGroupType.Business)]
    public class GetServerInfoCommand : ViewCommand
    {
        public override bool CanExecute(LogicalView view)
        {
            return RafyEnvironment.Location.IsWPFUI && !RafyEnvironment.Location.ConnectDataDirectly;
        }

        public override void Execute(LogicalView view)
        {
            view.DataLoader.LoadDataAsync();
        }
    }

    [Command(Label = "显示本机状态", ToolTip = "显示本机当前已经加载数据的状态", GroupType = CommandGroupType.Business)]
    public class GetClientInfoCommand : ViewCommand
    {
        public override void Execute(LogicalView view)
        {
            view.DataLoader.LoadDataAsync(() => RF.Concrete<FrameworkInfoItemRepository>().GetAllOnClient());
        }
    }

    [Command(Label = "加载所有实体", GroupType = CommandGroupType.Business)]
    public class LoadAllEntitiesCommand : ViewCommand
    {
        public override void Execute(LogicalView view)
        {
            CommonModel.Entities.EnsureAllLoaded();
            view.DataLoader.LoadDataAsync(() => RF.Concrete<FrameworkInfoItemRepository>().GetAllOnClient());
        }
    }
}