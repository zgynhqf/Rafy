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
using System.Text;
using OEA.WPF.Command;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.Module.WPF;
using OEA.Library;
using System.Windows;

namespace JXC.Commands
{
    public abstract class DeleteBill : ListViewCommand
    {
        protected DeleteService Service;

        public override bool CanExecute(ListObjectView view)
        {
            return view.SelectedEntities.Count == 1;
        }

        public override void Execute(ListObjectView view)
        {
            var res = App.MessageBox.Show(string.Format("确定删除该{0}，删除后不可还原？", view.Meta.Label), MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                this.Service.ItemId = view.Current.Id;
                this.Service.Invoke();

                var result = this.Service.Result;
                if (result) { view.DataLoader.ReloadDataAsync(); }

                App.MessageBox.Show(result.Message);
            }
        }
    }
}