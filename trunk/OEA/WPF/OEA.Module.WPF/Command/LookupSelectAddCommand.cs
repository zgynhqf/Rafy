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
using OEA.Editors;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;

using OEA.Module.WPF.Editors;
using OEA.WPF;
using OEA.WPF.Command;
using SimpleCsla.Core;

namespace OEA.Module.WPF.Command
{
    //[Command(ImageName = "LookupSelectAdd.bmp", Label = "选择新增")]
    public abstract class LookupSelectAddCommand : ListViewCommand
    {
        public override bool CanExecute(ListObjectView view)
        {
            return view.Data != null;
        }

        protected Type TargetEntityType { get; set; }

        protected IRefProperty RefProperty { get; set; }

        public override void Execute(ListObjectView view)
        {
            if (this.TargetEntityType == null) throw new ArgumentNullException("this.TargetEntityType");
            if (this.RefProperty == null) throw new ArgumentNullException("this.RefProperty");

            var listView = AutoUI.ViewFactory.CreateListObjectView(this.TargetEntityType);

            listView.DataLoader.GetObjectAsync();

            var result = App.Current.Windows.ShowDialog(listView.Control, w =>
            {
                w.ResizeMode = ResizeMode.CanResize;
                w.Title = this.CommandInfo.Label;
                w.Width = 800;
                w.Height = 600;
            });

            if (result == WindowButton.Yes)
            {
                var activeObjects = listView.GetActiveObjects();
                for (int i = activeObjects.Count - 1; i >= 0; i--)
                {
                    var src = activeObjects[i] as Entity;
                    //如果已经存在，则退出
                    bool eixst = false;
                    foreach (var item in view.Data)
                    {
                        var entity = item.GetLazyRef(this.RefProperty).Entity;
                        if (entity.Id == src.Id)
                        {
                            eixst = true;
                            break;
                        }
                    }
                    if (eixst) continue;

                    //把选中对象赋值到新增对象的引用属性上
                    var newEntity = view.AddNew(false);
                    newEntity.GetLazyRef(this.RefProperty).Entity = src;
                }

                view.RefreshControl();
            }
        }
    }
}