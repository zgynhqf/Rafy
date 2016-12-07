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
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JXC.WPF.Templates;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Command;
using Rafy.WPF.Controls;

namespace JXC.Commands
{
    public abstract class AddBill : ListViewCommand
    {
        public AddBill()
        {
            this.Template = new BillTemplate();
        }

        protected UITemplate Template;

        protected AddService Service;

        public override void Execute(ListLogicalView view)
        {
            //创建一个临时的拷贝数据
            var tmpEntity = view.CreateNewItem();

            //弹出窗体显示详细面板
            this.Template.EntityType = view.EntityType;
            var ui = this.Template.CreateUI();

            var detailView = ui.MainView.CastTo<DetailLogicalView>();
            detailView.Data = tmpEntity;

            App.Windows.ShowDialog(ui.Control, w =>
            {
                w.Buttons = ViewDialogButtons.YesNo;
                w.Title = this.Meta.Label.Translate() + " " + view.Meta.Label.Translate();

                //验证
                w.ValidateOperations += (o, e) =>
                {
                    var broken = tmpEntity.Validate();
                    if (broken.Count > 0)
                    {
                        App.MessageBox.Show(broken.ToString(), "属性错误".Translate(), MessageBoxButton.OK, MessageBoxImage.Error);
                        e.Cancel = true;
                    }
                };

                //焦点
                w.Loaded += (o, e) =>
                {
                    var txt = w.GetVisualChild<TextBox>();
                    if (txt != null) { Keyboard.Focus(txt); }
                };

                //窗口在数据改变后再关闭窗口，需要提示用户是否保存。
                (tmpEntity as IDirtyAware).MarkSaved();
                w.Closing += (o, e) =>
                {
                    if (w.DialogResult != true)
                    {
                        if (tmpEntity.IsDirty)
                        {
                            var res = App.MessageBox.Show("直接退出将不会保存数据，是否退出？".Translate(), MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            e.Cancel = res == MessageBoxResult.No;
                        }
                    }
                    else
                    {
                        //如果保存的过程中发生异常，不要关闭当前窗口
                        try
                        {
                            this.OnServiceInvoking(detailView, e);
                            if (e.Cancel) return;

                            tmpEntity.PersistenceStatus = PersistenceStatus.New;

                            this.Service.Item = tmpEntity;
                            this.Service.Invoke();

                            App.MessageBox.Show(this.Service.Result.Message.Translate());

                            if (!this.Service.Result.Success)
                            {
                                e.Cancel = true;
                            }
                            else
                            {
                                tmpEntity.Id = this.Service.NewId;
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.Alert();
                            e.Cancel = true;
                        }

                        if (!e.Cancel)
                        {
                            view.DataLoader.ReloadDataAsync(() =>
                            {
                                view.Current = view.Data.FirstOrDefault(en => en.Id == tmpEntity.Id);
                            });
                        }
                    }
                };
            });
        }

        protected virtual void OnServiceInvoking(DetailLogicalView detailView, CancelEventArgs e) { }
    }
}