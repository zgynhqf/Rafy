/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110303
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100303
 * 向子类公布虚方法，提供更改元数据的扩展接口。 胡庆访 20110727
 * 
*******************************************************/

using System.Windows;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.MetaModel.Attributes;
using OEA.Module.WPF;
using OEA.Module.WPF.Behaviors;
using OEA.Module.WPF.Controls;
using System;
using System.Windows.Controls;

namespace OEA.WPF.Command
{
    [Command(Label = "修改", GroupType = CommandGroupType.Edit)]
    public class EditDetailCommand : PopupDetailCommand
    {
        public override bool CanExecute(ListObjectView view)
        {
            return base.CanExecute(view) &&
                view.Current != null;
        }

        public override void Execute(ListObjectView view)
        {
            //创建一个临时的拷贝数据
            var listEntity = view.Current;
            var tmp = Entity.New(view.EntityType);
            tmp.Clone(listEntity);

            var evm = view.Meta;
            var res = PopupEditingDialog(evm, tmp, w =>
            {
                w.Title = this.CommandInfo.Label + evm.Label;
            });

            if (res == WindowButton.Yes)
            {
                //修改按钮如果使用新的 Id，则它下面子对象的父外键都将是错误的值。
                listEntity.Clone(tmp, new CloneOptions(
                    CloneActions.NormalProperties | CloneActions.RefEntities
                    ));

                listEntity.MarkModified();

                view.RefreshControl();
            }
        }
    }
}
