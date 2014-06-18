using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.MetaModel.Attributes;
using Rafy.WPF.Command;
using Rafy.WPF;
using Rafy.Domain;

namespace Demo.WPF.Commands
{
    [Command(Label = "命令测试")]
    public class DemoCommand : ListViewCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            return view.Current != null;
        }

        public override void Execute(ListLogicalView view)
        {
            //调用服务端查询库中的所有书籍量
            var svc = ServiceFactory.Create<CountServerBookJsonService>();
            svc.Invoke();
            App.MessageBox.Show("库中一共有书籍：" + svc.BookCount + "本。");

            //view.Data = null;//清空列表数据
            //view.DataLoader.ReloadDataAsync();//重新查询列表的数据
            //view.Data = RF.Create<BookCategory>().GetAll();//指定列表的数据
            //view.Current = view.Data[0];//选中第一行。
            //view.Filter = e => (e as BookCategory).Name.Contains("人文");//过滤
            //var newEntity = view.AddNew(true);//添加一行新数据
            ////其它....
        }
    }
}