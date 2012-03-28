/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111107
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111107
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using OEA.Library;
using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.MetaModel.View;

namespace OEA.Module.WPF
{
    public abstract class DynamicPropertyHelperBase<TEntity, TProperty>
        where TEntity : Entity
    {
        private ListObjectView _view;

        private Dictionary<string, Property<TProperty>> _dynamicProperties;

        /// <summary>
        /// 根据所给列名，扩展指定的视图
        /// </summary>
        /// <param name="view"></param>
        /// <param name="yLabels"></param>
        protected void Refresh(ListObjectView view, IList<string> yLabels)
        {
            if (yLabels == null || yLabels.Count == 0) return;

            this._view = view;
            this._dynamicProperties = new Dictionary<string, Property<TProperty>>();

            this.RefreshDynamicControl(yLabels);
        }

        private void RefreshDynamicControl(IList<string> yLabels)
        {
            P<TEntity>.UnRegisterAllRuntimeProperties();

            //这里改为动态列。
            var evm = this._view.Meta;
            for (int i = 0; i < yLabels.Count; i++)
            {
                var label = yLabels[i];

                var mp = P<TEntity>.RegisterExtension<TProperty>("DynamicProperty_" + i);

                CommonModel.Entities.CreateExtensionPropertyMeta(mp, evm.EntityMeta);
                var ep = UIModel.Views.CreateExtensionPropertyViewMeta(mp, evm);
                ep.Readonly(true).ShowIn(ShowInWhere.List).HasLabel(yLabels[i]);

                this._dynamicProperties[label] = mp;
            }

            //替换控件
            this.ReplaceControl(evm);

            //设置值
            this.SyncDynamicValues();
        }

        /// <summary>
        /// 在不需要动态列的时候，可调用此方法把界面还原
        /// </summary>
        protected void RestoreControl()
        {
            this.ReplaceControl(this._view.Meta);
        }

        private void ReplaceControl(EntityViewMeta evm)
        {
            var newControl = AutoUI.BlockUIFactory.CreateTreeListControl(evm, ShowInWhere.List);

            this._view.ReplaceControl(newControl);
        }

        /// <summary>
        /// 保证视图所要显示的数据都已经把扩展列的值设置好。
        /// </summary>
        /// <param name="userBQItemsView"></param>
        private void SyncDynamicValues()
        {
            var data = this._view.Data as EntityList;

            if (data != null && data.Count > 0)
            {
                foreach (TEntity xEntity in data)
                {
                    //对于每一个扩展列，找到对应 X、Y 的 value 值，填入值。
                    foreach (var kv in this._dynamicProperties)
                    {
                        var yLabel = kv.Key;
                        var mp = kv.Value;

                        var value = this.GetValue(xEntity, yLabel);

                        xEntity.SetProperty(mp, value, ManagedPropertyChangedSource.FromPersistence);
                    }
                }
            }
        }

        /// <summary>
        /// 获取对应 X、Y 的 value 值，填入值。
        /// </summary>
        /// <param name="xEntity">X 是当前行的实体</param>
        /// <param name="yLabel">Y 是当前行实体所需要显示的列名</param>
        /// <returns></returns>
        protected abstract TProperty GetValue(TEntity xEntity, string yLabel);
    }
}
