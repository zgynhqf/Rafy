/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100326
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100326
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Command;
using Rafy.WPF.Editors;

namespace Rafy.WPF.Command
{
    /// <summary>
    /// 选择新增功能按钮的基类。
    /// </summary>
    public abstract class LookupSelectAddCommand : ListViewCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            return view.Data != null;
        }

        protected Type TargetEntityType { get; set; }

        protected IRefProperty RefProperty { get; set; }

        public override void Execute(ListLogicalView view)
        {
            if (this.RefProperty == null) throw new ArgumentNullException("this.RefProperty");

            var ui = this.GenerateSelectionUI();

            var result = this.PopupSelectionWindow(ui);

            if (result == WindowButton.Yes)
            {
                foreach (var src in ui.MainView.GetSelectedEntities())
                {
                    //如果已经存在，则退出
                    bool eixst = false;
                    foreach (var item in view.Data)
                    {
                        var entity = item.GetRefEntity(this.RefProperty.RefEntityProperty);
                        if (entity.Id == src.Id)
                        {
                            eixst = true;
                            break;
                        }
                    }
                    if (eixst) continue;

                    this.AddSelection(view, src);
                }

                this.Complete(view);
            }
        }

        /// <summary>
        /// 通过实体类生成选择界面。
        /// 重写时注意，这个界面的主块应该是一个列表视图
        /// </summary>
        /// <returns></returns>
        protected virtual ControlResult GenerateSelectionUI()
        {
            if (this.TargetEntityType == null) throw new ArgumentNullException("this.TargetEntityType");
            var listView = AutoUI.ViewFactory.CreateListView(this.TargetEntityType);
            listView.IsReadOnly = ReadOnlyStatus.ReadOnly;
            listView.DataLoader.LoadDataAsync();
            return listView;
        }

        /// <summary>
        /// 子类重写此方法实现自己的弹窝逻辑。
        /// </summary>
        /// <param name="ui"></param>
        /// <returns></returns>
        protected virtual WindowButton PopupSelectionWindow(ControlResult ui)
        {
            return App.Windows.ShowDialog(ui.Control, w =>
            {
                w.Title = this.Meta.Label.Translate();
            });
        }

        /// <summary>
        /// 为被选择的对象添加一个引用实体属性值
        /// </summary>
        /// <param name="view"></param>
        /// <param name="selected"></param>
        /// <returns></returns>
        protected virtual Entity AddSelection(ListLogicalView view, Entity selected)
        {
            //把选中对象赋值到新增对象的引用属性上
            var newEntity = view.AddNew(false);
            newEntity.SetRefEntity(this.RefProperty.RefEntityProperty, selected);
            return newEntity;
        }

        /// <summary>
        /// 最后完成本次选择。
        /// </summary>
        /// <param name="view"></param>
        protected virtual void Complete(ListLogicalView view)
        {
            view.RefreshControl();
        }
    }
}