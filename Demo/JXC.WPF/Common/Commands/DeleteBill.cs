/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120418
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120418
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Command;

namespace JXC.Commands
{
    public abstract class DeleteBill : ListViewCommand
    {
        protected DeleteService Service;

        public override bool CanExecute(ListLogicalView view)
        {
            return view.SelectedEntities.Count == 1;
        }

        public override void Execute(ListLogicalView view)
        {
            var res = App.MessageBox.Show(string.Format("确定删除该{0}，删除后不可还原？".Translate(), view.Meta.Label), MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                this.Service.ItemId = (int)view.Current.Id;
                this.Service.Invoke();

                var result = this.Service.Result;
                if (result) { view.DataLoader.ReloadDataAsync(); }

                App.MessageBox.Show(result.Message.Translate());
            }
        }
    }
}