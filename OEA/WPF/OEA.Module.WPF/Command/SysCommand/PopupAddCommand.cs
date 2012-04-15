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
using System.Windows.Controls;


namespace OEA.WPF.Command
{
    [Command(ImageName = "Add.bmp", Label = "添加", GroupType = CommandGroupType.Edit)]
    public class PopupAddCommand : PopupDetailCommand
    {
        public override bool CanExecute(ListObjectView view)
        {
            return view.CanAddItem();
        }

        public override void Execute(ListObjectView view)
        {
            //创建一个临时的拷贝数据
            var tmp = view.CreateNewItem();

            var evm = view.Meta;
            var result = PopupEditingDialog(evm, tmp, w =>
            {
                w.Title = this.CommandInfo.Label + evm.Label;
            });

            //如果没有点击确定，则删除刚才添加的记录。
            if (result == WindowButton.Yes)
            {
                //先添加一行记录
                var curEntity = view.AddNew(false);

                var oldTreeCode = string.Empty;
                if (evm.EntityMeta.IsTreeEntity)
                {
                    oldTreeCode = curEntity.TreeCode;
                }

                curEntity.Clone(tmp, new CloneOptions(
                    CloneActions.NormalProperties | CloneActions.RefEntities
                    ));

                //如果用户没有设置树型编码，则把树型编码还原到 Clone 之前系统自动生成的编码
                if (!string.IsNullOrEmpty(oldTreeCode) &&
                    string.IsNullOrEmpty(curEntity.TreeCode) &&
                    view.Data.AutoTreeCodeEnabled)
                {
                    curEntity.TreeCode = oldTreeCode;
                }

                this.OnDataCloned(curEntity, tmp);

                view.RefreshControl();
                view.Current = curEntity;
            }
        }

        #region DataCloned

        /// <summary>
        /// 临时对象数据拷贝时发生此事件
        /// </summary>
        public event EventHandler<DataClonedEventArgs> DataCloned;

        protected virtual void OnDataCloned(Entity newEntity, Entity tmp)
        {
            var handler = this.DataCloned;
            if (handler != null) handler(this, new DataClonedEventArgs(newEntity, tmp));
        }

        public class DataClonedEventArgs : EventArgs
        {
            public DataClonedEventArgs(Entity newEntity, Entity tmp)
            {
                this.NewEntity = newEntity;
                this.TempEntity = tmp;
            }

            public Entity NewEntity { get; private set; }
            public Entity TempEntity { get; private set; }
        }

        #endregion
    }
}