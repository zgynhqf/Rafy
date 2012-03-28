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

namespace OEA.WPF.Command
{
    [Command(Label = "修改", GroupType = CommandGroupType.Edit)]
    public class EditDetailCommand : ListViewCommand
    {
        public override bool CanExecute(ListObjectView view)
        {
            return base.CanExecute(view) &&
                view.Current != null;
        }

        public override void Execute(ListObjectView view)
        {
            //创建一个临时的拷贝数据
            var listEntity = view.Current as Entity;
            var tmp = RF.Create(view.EntityType).New();
            tmp.Clone(listEntity);
            tmp.CheckRules();

            var evm = this.GetViewMeta(view);
            var res = EditAddHelper.ShowEditingDialog(evm, tmp, w =>
            {
                w.Title = this.CommandInfo.Label;
            }, () => this.CheckTemporaryEntityError(view, tmp));

            if (res == WindowButton.Yes)
            {
                EditAddHelper.CloneWithCallback(listEntity, tmp);

                listEntity.MarkDirty();

                view.RefreshControl();
            }
        }

        protected virtual string CheckTemporaryEntityError(ListObjectView view, object tmp) { return null; }

        /// <summary>
        /// 获取用于编辑的实体视图模型，子类重写此方法可更改元模型。
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        protected virtual EntityViewMeta GetViewMeta(ListObjectView view)
        {
            return view.Meta;
        }
    }
}
