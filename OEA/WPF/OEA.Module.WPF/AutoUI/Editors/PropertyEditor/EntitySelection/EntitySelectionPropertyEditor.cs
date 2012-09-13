/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120408
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120408
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;
using System.Windows.Data;
using System.Collections;
using OEA.MetaModel;
using System.Windows;
using OEA.ManagedProperty;
using OEA.MetaModel.View;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 实体选择属性编辑器的基类
    /// </summary>
    public abstract class EntitySelectionPropertyEditor : WPFPropertyEditor
    {
        #region 事件

        /// <summary>
        /// 属性变化前发生此事件。
        /// </summary>
        public event EventHandler ReferenceEntityChanging;

        private void OnReferenceEntityChanging()
        {
            var handler = this.ReferenceEntityChanging;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        protected override Binding CreateBinding()
        {
            var property = this.Meta;

            //如果被绑定的属性不是实体引用属性，则需要使用 TwoWay 把值写回去。
            var bindingMode = BindingMode.OneWay;
            if (string.IsNullOrEmpty(property.SelectionViewMeta.RefEntityProperty))
            {
                if (this.PropertyCanWrite) { bindingMode = BindingMode.TwoWay; }
            }

            var binding = new Binding()
            {
                Mode = bindingMode,
                Path = new PropertyPath(property.DisplayPath(), property.PropertyMeta.ManagedProperty),
                ValidationRules = { ManagedProeprtyValidationRule.Instance }
            };

            return binding;
        }

        #region SyncSelectionToValue

        /// <summary>
        /// 当编辑器已经先选中某个值时，需要使用此方法通知属性变更。
        /// </summary>
        /// <param name="selecteEntity"></param>
        protected void SyncSelectionToValue(IList<Entity> selectedEntities)
        {
            if (selectedEntities == null) throw new ArgumentNullException("selectedEntities");
            if (!(selectedEntities is IList))
            {
                throw new ArgumentException("传入的 selectedEntities 参数必须实现 IList 非泛型接口。");
            }

            var curEntity = this.Context.CurrentObject;
            var rvm = this.Meta.SelectionViewMeta;

            if (this.IsMultiSelection)
            {
                this.SyncSelectionToValue_Multiple(curEntity, rvm, selectedEntities);
            }
            else
            {
                this.SyncSelectionToValue_Single(curEntity, rvm, selectedEntities);
            }

            if (rvm.RefSelectedCallBack != null)
            {
                rvm.RefSelectedCallBack(curEntity, selectedEntities as IList);
            }
        }

        private void SyncSelectionToValue_Single(Entity curEntity, SelectionViewMeta rvm, IList<Entity> selectedEntities)
        {
            if (curEntity != null)
            {
                //引用属性，应该先尝试设置实体属性，再设置 Id 属性。
                var rp = this.Meta.PropertyMeta.ManagedProperty as IRefProperty;
                if (selectedEntities.Count > 0)
                {
                    var selecteEntity = selectedEntities[0] as Entity;
                    if (rp != null)
                    {
                        this.OnReferenceEntityChanging();
                        curEntity.GetLazyRef(rp).Entity = selecteEntity;
                    }
                    else
                    {
                        this.PropertyValue = this.GetSelectedValue(selecteEntity);
                        if (rvm.RefIdHost != null)
                        {
                            curEntity.SetProperty(rvm.RefIdHost, selecteEntity.Id, ManagedPropertyChangedSource.FromUIOperating);
                        }
                    }
                }
                else
                {
                    if (rp != null)
                    {
                        this.OnReferenceEntityChanging();
                        curEntity.GetLazyRef(rp).Entity = null;
                    }
                    else
                    {
                        this.PropertyValue = null;
                        if (rvm.RefIdHost != null)
                        {
                            curEntity.SetProperty(rvm.RefIdHost, null, ManagedPropertyChangedSource.FromUIOperating);
                        }
                    }
                }
            }
        }

        private void SyncSelectionToValue_Multiple(Entity curEntity, SelectionViewMeta rvm, IList<Entity> selectedEntities)
        {
            var result = string.Join(rvm.SplitterIfMulti, selectedEntities.Select(i => this.GetSelectedValue(i)));

            //赋值给this.PropertyValue
            this.PropertyValue = result;

            //此时这个属性应该是一个 int 数组类型
            if (rvm.RefIdHost != null)
            {
                var idArray = selectedEntities.Select(i => i.Id).ToArray();
                curEntity.SetProperty(rvm.RefIdHost, idArray, ManagedPropertyChangedSource.FromUIOperating);
            }
        }

        #endregion

        #region SyncValueToSelection

        /// <summary>
        /// 根据当前的值（PropertyValue），找到并定位到当前对象
        /// </summary>
        protected void SyncValueToSelection(IListObjectView listView)
        {
            if (listView == null) throw new ArgumentNullException("listView");

            var items = listView.Data;
            if (items == null || items.Count == 0) return;

            //找到值对应的数据项
            var selectedItems = new List<Entity>();
            if (this.Context.CurrentObject != null && this.Meta != null)
            {
                //根据设置的 SelectedValuePath 来对比属性值 this.PropertyValue，如果相同，则找到对应的CurrentObject
                var targetValue = this.PropertyValue;
                if (this.IsMultiSelection && targetValue != null)
                {
                    var splitter = this.Meta.SelectionViewMeta.SplitterIfMulti;
                    var values = targetValue.ToString().Split(
                        new string[] { splitter }, StringSplitOptions.RemoveEmptyEntries
                        );
                    foreach (var selectedValue in values)
                    {
                        AddToListBySelectedValue(items, selectedItems, selectedValue);
                    }
                }
                else
                {
                    AddToListBySelectedValue(items, selectedItems, targetValue);
                }
            }

            //定位 SelectedObjects
            var selectedObjects = listView.SelectedEntities;
            selectedObjects.Clear();
            foreach (var item in selectedItems) { selectedObjects.Add(item); }
        }

        private void AddToListBySelectedValue(IList items, List<Entity> selectedItems, object targetValue)
        {
            var selectedItem = items.OfType<Entity>().FirstOrDefault(item =>
                 object.Equals(targetValue, this.GetSelectedValue(item))
                 );
            if (selectedItem != null) selectedItems.Add(selectedItem);
        }

        #endregion

        /// <summary>
        /// 根据SelectedValuePath指定的值，获取目标属性值
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private object GetSelectedValue(Entity entity)
        {
            return this.Meta.SelectionViewMeta.GetSelectedValue(entity);
        }

        /// <summary>
        /// 当前是否处于多选的模式下。
        /// </summary>
        protected bool IsMultiSelection
        {
            get { return this.Meta.SelectionViewMeta.SelectionMode == EntitySelectionMode.Multiple; }
        }
    }
}