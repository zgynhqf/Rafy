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
    /// 删除列表中选中的对象
    /// </summary>
    [Command(ImageName = "Delete.bmp", Label = "删除", GroupType = CommandGroupType.Edit)]
    public class DeleteListObjectCommand : ListViewCommand
    {
        public override bool CanExecute(ListObjectView view)
        {
            return view.Current != null && view.Data.AllowRemove;
        }

        public override void Execute(ListObjectView view)
        {
            var dataList = view.Data;
            var dealingTree = dataList.SupportTree;
            var list = view.SelectedEntities.ToArray();
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
        }
    }
}