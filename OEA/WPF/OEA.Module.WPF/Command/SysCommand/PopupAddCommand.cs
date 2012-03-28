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
using System.ComponentModel;


namespace OEA.WPF.Command
{
    [Command(ImageName = "Add.bmp", Label = "添加", GroupType = CommandGroupType.Edit)]
    public class PopupAddCommand : AddCommand
    {
        public override void Execute(ListObjectView view)
        {
            //创建一个临时的拷贝数据
            var tmp = RF.Create(view.EntityType).New();

            this.OnTmpEntityCreated(tmp);

            var evm = this.GetViewMeta(view);

            var result = EditAddHelper.ShowEditingDialog(evm, tmp, w =>
            {
                w.Title = "添加记录";
                this.OnWindowShowing(w);
            }, () => this.CheckTemporaryEntityError(view, tmp));

            //如果没有点击确定，则删除刚才添加的记录。
            if (result == WindowButton.Yes)
            {
                //先添加一行记录
                var curEntity = view.AddNew(false);

                EditAddHelper.CloneWithCallback(curEntity, tmp);

                this.OnDataCloned(curEntity, tmp);

                view.RefreshControl();
                view.Current = curEntity;

                AfterNotifyEditCompleted(view);
            }
        }

        /// <summary>
        /// 获取用于编辑的实体视图模型，子类重写此方法可更改元模型。
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        protected virtual EntityViewMeta GetViewMeta(ListObjectView view)
        {
            return view.Meta;
        }

        /// <summary>
        /// 检测临时编辑对象的状态。
        /// </summary>
        /// <param name="view"></param>
        /// <param name="tmp"></param>
        /// <returns>
        /// 如果状态不可用，则返回提示信息。否则，返回null。
        /// </returns>
        protected virtual string CheckTemporaryEntityError(ListObjectView view, object tmp) { return null; }

        /// <summary>
        /// 子类重写此方法来设置默认值。
        /// </summary>
        /// <param name="tmp"></param>
        protected virtual void OnTmpEntityCreated(Entity tmp) { }

        /// <summary>
        /// 窗体弹出前发生的事件
        /// </summary>
        /// <returns></returns>
        protected virtual void OnWindowShowing(Window w) { }

        /// <summary>
        /// 当前窗口数据刷新之后
        /// </summary>
        /// <param name="w"></param>
        protected virtual void AfterNotifyEditCompleted(ListObjectView view) { }

        /// <summary>
        /// 临时对象数据拷贝时发生
        /// </summary>
        /// <param name="newEntity"></param>
        /// <param name="tmp"></param>
        protected virtual void OnDataCloned(Entity newEntity, Entity tmp) { }
    }

    internal static class EditAddHelper
    {
        internal static WindowButton ShowEditingDialog(
            EntityViewMeta entityViewMeta, Entity tmpEntity,
            Action<ViewDialog> windowSetter, Func<string> checkError
            )
        {
            //弹出窗体显示详细面板
            var detailView = AutoUI.ViewFactory.CreateDetailObjectView(entityViewMeta);
            detailView.Control.VerticalAlignment = VerticalAlignment.Top;
            detailView.Data = tmpEntity;

            var result = App.Current.Windows.ShowDialog(detailView.Control, w =>
            {
                w.Buttons = ViewDialogButtons.YesNo;
                w.SizeToContent = SizeToContent.Height;
                w.MinHeight = 100;
                w.MinWidth = 200;
                w.Width = 400 * detailView.ColumnsCount;
                w.ValidateOperations += (o, e) =>
                {
                    var error = checkError();
                    if (error != null)
                    {
                        App.Current.MessageBox.Show("提示", error);
                        e.Cancel = true;
                        return;
                    }

                    tmpEntity.CheckRules();
                    if (!tmpEntity.IsValid)
                    {
                        App.Current.MessageBox.Show("属性错误", tmpEntity.BrokenRulesCollection[0].Description);
                        e.Cancel = true;
                    }
                };

                windowSetter(w);
            });

            return result;
        }

        internal static void CloneWithCallback(Entity realEntity, Entity tmp)
        {
            var callBackEntity = realEntity as IAddOrEditCommandCallback;
            if (callBackEntity != null) { callBackEntity.BeforeClone(tmp); }

            //修改按钮如果使用新的Id，则它下面子对象的父外键都将是错误的值。
            realEntity.Clone(tmp, new CloneOptions(
                CloneActions.NormalProperties | CloneActions.RefEntities
                ));

            if (callBackEntity != null) { callBackEntity.AfterClone(tmp); }
        }
    }
}