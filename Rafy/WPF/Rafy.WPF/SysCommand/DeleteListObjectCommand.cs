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
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.MetaModel.Attributes;
using Rafy.WPF;

namespace Rafy.WPF.Command
{
    /// <summary>
    /// 删除列表中选中的对象
    /// </summary>
    [Command(ImageName = "Erase.png", Label = "删除", GroupType = CommandGroupType.Edit)]
    public class DeleteListObjectCommand : ListViewCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            return view.Current != null;
        }

        public override void Execute(ListLogicalView view)
        {
            var dataList = view.Data;
            var isTree = dataList.SupportTree;
            var selectedItems = view.SelectedEntities.ToArray();
            for (int i = selectedItems.Length - 1; i >= 0; i--)
            {
                var selectedItem = selectedItems[i];

                //如果是树形,则级联删除所有子节点
                if (isTree)
                {
                    var node = selectedItem as ITreeComponent;
                    var parent = node.TreeComponentParent;
                    if (parent.ComponentType == TreeComponentType.NodeList)
                    {
                        dataList.Remove(selectedItem);
                    }
                    else if (parent.ComponentType == TreeComponentType.TreeChildren)
                    {
                        var parentList  = parent as Entity.EntityTreeChildren;
                        parentList.Remove(selectedItem);
                    }
                }
                else
                {
                    dataList.Remove(selectedItem);
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