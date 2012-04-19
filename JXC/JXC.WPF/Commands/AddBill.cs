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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JXC.WPF.Templates;
using OEA;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.Module.WPF;
using OEA.Module.WPF.Controls;
using OEA.WPF.Command;
using OEA.Library;

namespace JXC.Commands
{
    public abstract class AddBill : ListViewCommand
    {
        public AddBill()
        {
            this.Template = new BillTemplate();
        }

        protected CustomTemplate Template;

        protected AddService Service;

        public override void Execute(ListObjectView view)
        {
            //创建一个临时的拷贝数据
            var tmpEntity = view.CreateNewItem();

            //弹出窗体显示详细面板
            var ui = this.Template.CreateUI(view.EntityType);

            var detailView = ui.MainView.CastTo<DetailObjectView>();
            detailView.Data = tmpEntity;

            App.Windows.ShowDialog(ui.Control, w =>
            {
                w.Buttons = ViewDialogButtons.YesNo;
                w.Title = this.CommandInfo.Label + view.Meta.Label;

                //验证
                w.ValidateOperations += (o, e) =>
                {
                    var broken = tmpEntity.ValidationRules.Validate();
                    if (broken.Count > 0)
                    {
                        App.MessageBox.Show(broken.ToString(), "属性错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
                tmpEntity.MarkOld();
                w.Closing += (o, e) =>
                {
                    if (w.DialogResult != true)
                    {
                        if (tmpEntity.IsDirty)
                        {
                            var res = App.MessageBox.Show("直接退出将不会保存数据，是否退出？", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            e.Cancel = res == MessageBoxResult.No;
                        }
                    }
                    else
                    {
                        //如果保存的过程中发生异常，不要关闭当前窗口
                        try
                        {
                            tmpEntity.MarkNew();

                            this.Service.Item = tmpEntity;
                            this.Service.Invoke();

                            App.MessageBox.Show(this.Service.Result.Message);

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
    }
}