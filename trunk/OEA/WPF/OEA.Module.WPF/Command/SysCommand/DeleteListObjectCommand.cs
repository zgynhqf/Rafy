/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111215
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111215
 * 
*******************************************************/

using System.ComponentModel;
using System.Linq;
using System.Windows;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.MetaModel.Attributes;
using OEA.Module.WPF;

namespace OEA.WPF.Command
{
    /// <summary>
    /// 删除列表中所有的根对象
    /// </summary>
    [Command(ImageName = "Delete.bmp", Label = "删除", GroupType = CommandGroupType.Edit)]
    public class DeleteListObjectCommand : ListViewCommand
    {
        private string _message = "确定删除当前记录?";

        public string Message
        {
            get { return this._message; }
            set { this._message = value; }
        }

        public override bool CanExecute(ListObjectView view)
        {
            if (base.CanExecute(view) == false)
            {
                return false;
            }
            var bindingList = view.Data as IBindingList;
            if (bindingList == null || bindingList.AllowRemove == false)
            {
                return false;
            }
            return view.Current != null;
        }

        public override void Execute(ListObjectView view)
        {
            //var result = App.MessageBox.Show("确认删除", this.Message, MessageBoxButton.YesNo);
            //if (result == MessageBoxResult.Yes)
            //{
            var dataList = view.Data as EntityList;
            var dealingTree = dataList.SupportTree;
            var list = view.SelectedEntities.OfType<Entity>().ToArray();
            for (int i = list.Length - 1; i >= 0; i--)
            {
                var item = list[i];

                //如果是树形,则级联删除所有子节点
                if (dealingTree)
                {
                    var children = item.GetRecurChildren();
                    //先删除细记录再删除主记录
                    for (int j = children.Count - 1; j >= 0; j--)
                    {
                        var model = children[j] as Entity;

                        dataList.Remove(model);
                    }
                }
                else
                {
                    dataList.Remove(item);
                }
            }

            if (dataList.Count > 0)
            {
                view.Current = dataList[0] as Entity;
            }
            else
            {
                view.Current = null;
            }

            view.RefreshControl();
            //}
        }
    }
}
