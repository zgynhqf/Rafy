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
using Rafy.Domain;
using System.Windows.Data;
using System.Collections;
using Rafy.MetaModel;
using System.Windows;
using Rafy.ManagedProperty;
using Rafy.MetaModel.View;

namespace Rafy.WPF.Editors
{
    /// <summary>
    /// 实体选择属性编辑器的基类
    /// </summary>
    public abstract class EntitySelectionPropertyEditor : PropertyEditor
    {
        #region 引用实体变更 事件

        /// <summary>
        /// 引用实体变化前发生此事件。
        /// 
        /// 只有当本编辑器正在对引用实体属性进行设置时才会发生此事件。
        /// </summary>
        public event EventHandler ReferenceEntityChanging;

        private void OnReferenceEntityChanging()
        {
            var handler = this.ReferenceEntityChanging;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// 引用实体变化后发生此事件。
        /// 
        /// 只有当本编辑器正在对引用实体属性进行设置时才会发生此事件。
        /// </summary>
        public event EventHandler ReferenceEntityChanged;

        private void OnReferenceEntityChanged()
        {
            var handler = this.ReferenceEntityChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        protected override Binding CreateBinding()
        {
            //如果被绑定的属性不是实体引用属性，则需要使用 TwoWay 把值写回去。
            var bindingMode = BindingMode.OneWay;
            var isEntityRef = this.Meta.PropertyMeta.ManagedProperty is IRefProperty;
            if (!isEntityRef)
            {
                if (this.PropertyCanWrite) { bindingMode = BindingMode.TwoWay; }
            }

            var binding = this.CreateBinding(bindingMode);

            return binding;
        }

        #region SyncSelectionToValue

        /// <summary>
        /// 当编辑器已经先选中某个值时，需要使用此方法通知属性变更。
        /// </summary>
        /// <param name="selecteEntity"></param>
        /// <returns>返回是否设置成功。（有可能值被属性自身给 cancel 掉。）</returns>
        protected SetSelectionResult SyncSelectionToValue(IList<Entity> selectedEntities)
        {
            var res = SetSelectionResult.Success;

            //参数判断
            if (selectedEntities == null) throw new ArgumentNullException("selectedEntities");
            if (!(selectedEntities is IList))
            {
                throw new ArgumentException("传入的 selectedEntities 参数必须实现 IList 非泛型接口。");
            }

            var curEntity = this.Context.CurrentObject;
            if (curEntity != null)
            {
                var svm = this.Meta.SelectionViewMeta;

                this.OnPropertyValueChanging();

                try
                {
                    //先关闭基类的属性变更事件，待所有属性全部变更完毕后，再在本函数的末尾一次性提交。
                    this.RaisePropertyChangeEvents = false;

                    if (this.IsMultiSelection)
                    {
                        res = this.SyncSelectionToValue_Multiple(curEntity, svm, selectedEntities);
                    }
                    else
                    {
                        res = this.SyncSelectionToValue_Single(curEntity, svm, selectedEntities);
                    }
                }
                finally
                {
                    this.RaisePropertyChangeEvents = true;
                }

                if (res == SetSelectionResult.Success)
                {
                    if (svm.RefSelectedCallBack != null)
                    {
                        svm.RefSelectedCallBack(curEntity, selectedEntities as IList);
                    }

                    this.OnPropertyValueChanged();
                }
            }

            return res;
        }

        /// <summary>
        /// 单选模式下的写值操作
        /// </summary>
        /// <param name="curEntity">正在编辑的实体。</param>
        /// <param name="svm">选择视图模型</param>
        /// <param name="selectedEntities">当前被选择的实体</param>
        private SetSelectionResult SyncSelectionToValue_Single(Entity curEntity, SelectionViewMeta svm, IList<Entity> selectedEntities)
        {
            bool success = false;

            //引用属性，应该先尝试设置实体属性，再设置 Id 属性。
            var rp = RefPropertyHelper.Find(this.Meta.PropertyMeta.ManagedProperty);
            if (selectedEntities.Count > 0)
            {
                var selectedEntity = selectedEntities[0] as Entity;
                if (rp != null)
                {
                    //如果 SelectedValuePath 是一个引用属性，或者直接就是一个实体属性，
                    //则应该获取相应的实体的值。
                    var valuePath = svm.SelectedValuePath;
                    if (valuePath != null)
                    {
                        var valuePathRP = RefPropertyHelper.Find(valuePath);
                        if (valuePathRP != null)
                        {
                            selectedEntity = selectedEntity.GetRefEntity(valuePathRP);
                        }
                        else if (rp.RefEntityType.IsAssignableFrom(valuePath.PropertyType))
                        {
                            selectedEntity = this.GetSelectedValue(selectedEntity) as Entity;
                        }
                    }

                    //设置实体到本引用属性上。
                    this.OnReferenceEntityChanging();
                    curEntity.SetRefEntity(rp, selectedEntity);
                    success = curEntity.GetRefEntity(rp) == selectedEntity;
                    if (success) { this.OnReferenceEntityChanged(); }
                }
                else
                {
                    var value = this.GetSelectedValue(selectedEntity);
                    this.PropertyValue = value;
                    success = this.PropertyValue == value;
                    if (success && svm.RefIdHost != null)
                    {
                        curEntity.SetProperty(svm.RefIdHost, selectedEntity.Id);
                    }
                }
            }
            else
            {
                if (rp != null)
                {
                    this.OnReferenceEntityChanging();
                    curEntity.SetRefEntity(rp, null);
                    success = curEntity.GetRefEntity(rp) == null;
                    if (success) { this.OnReferenceEntityChanged(); }
                }
                else
                {
                    this.PropertyValue = null;
                    success = this.PropertyValue == null;
                    if (success && svm.RefIdHost != null)
                    {
                        curEntity.SetProperty(svm.RefIdHost, null);
                    }
                }
            }

            return success ? SetSelectionResult.Success : SetSelectionResult.Cancel;
        }

        /// <summary>
        /// 多选模式下的写值操作
        /// </summary>
        /// <param name="curEntity">正在编辑的实体。</param>
        /// <param name="svm">选择视图模型</param>
        /// <param name="selectedEntities">当前被选择的实体</param>
        private SetSelectionResult SyncSelectionToValue_Multiple(Entity curEntity, SelectionViewMeta rvm, IList<Entity> selectedEntities)
        {
            var result = string.Join(rvm.Splitter, selectedEntities.Select(i => this.GetSelectedValue(i)));

            //赋值给this.PropertyValue
            this.PropertyValue = result;

            SetSelectionResult res = (this.PropertyValue as string) == result ? SetSelectionResult.Success : SetSelectionResult.Cancel;
            if (res == SetSelectionResult.Success)
            {
                //此时这个属性应该是一个 int 数组类型
                if (rvm.RefIdHost != null)
                {
                    var idArray = selectedEntities.Select(i => i.Id).ToArray();
                    curEntity.SetProperty(rvm.RefIdHost, idArray);
                }
            }

            return res;
        }

        /// <summary>
        /// 设置选择实体属性的情况
        /// </summary>
        public enum SetSelectionResult
        {
            /// <summary>
            /// 设置成功。
            /// </summary>
            Success,
            /// <summary>
            /// 有可能值被属性本身给取消了设置。
            /// </summary>
            Cancel
        }

        #endregion

        #region SyncValueToSelection

        /// <summary>
        /// 根据当前的值（PropertyValue），找到并定位到当前对象
        /// </summary>
        protected void SyncValueToSelection(ListLogicalView listView)
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
                    var splitter = this.Meta.SelectionViewMeta.Splitter;
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