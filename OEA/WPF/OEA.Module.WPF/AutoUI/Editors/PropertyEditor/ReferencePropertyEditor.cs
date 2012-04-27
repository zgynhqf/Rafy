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

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 引用实体属性编辑器的基类
    /// </summary>
    public abstract class ReferencePropertyEditor : WPFPropertyEditor
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
            var bindingMode = BindingMode.OneWay;
            var bindingPath = string.Empty;

            var property = this.Meta;
            if (string.IsNullOrEmpty(property.ReferenceViewInfo.RefEntityProperty))
            {
                bindingPath = property.Name;
                if (this.PropertyCanWrite) { bindingMode = BindingMode.TwoWay; }
            }
            else
            {
                bindingPath = this.Meta.BindingPath();
            }

            return new Binding()
            {
                Mode = bindingMode,
                Path = new PropertyPath(bindingPath)
            };
        }

        /// <summary>
        /// 当编辑器已经先选中某个值时，需要使用此方法通知属性变更。
        /// </summary>
        /// <param name="selecteEntity"></param>
        protected void SyncSelectionToValue(IList<Entity> selectedEntities)
        {
            if (!this.IsMultiSelection)
            {
                var curObj = this.Context.CurrentObject;
                if (curObj != null)
                {
                    var refInfo = this.Meta.ReferenceViewInfo;
                    if (selectedEntities.Count > 0)
                    {
                        var selecteEntity = selectedEntities[0] as Entity;

                        if (!string.IsNullOrEmpty(refInfo.RefEntityProperty))
                        {
                            this.OnReferenceEntityChanging();
                            curObj.SetPropertyValue(refInfo.RefEntityProperty, selecteEntity);
                        }
                        else
                        {
                            this.PropertyValue = this.GetSelectedValue(selecteEntity);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(refInfo.RefEntityProperty))
                        {
                            this.OnReferenceEntityChanging();
                            curObj.SetPropertyValue(refInfo.RefEntityProperty, null);
                        }
                        else
                        {
                            this.PropertyValue = null;
                        }
                    }
                }
            }
            else
            {
                var result = string.Empty;

                foreach (Entity item in selectedEntities)
                {
                    if (result.Length == 0)
                        result += this.GetSelectedValue(item);
                    else
                        result += this.Meta.ReferenceViewInfo.SplitterIfMulti + this.GetSelectedValue(item);
                }

                //赋值给this.PropertyValue
                this.PropertyValue = result;
            }
        }

        /// <summary>
        /// 根据当前的值（PropertyValue），找到并定位到当前对象
        /// </summary>
        protected void SyncValueToSelection(IListObjectView listView)
        {
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
                    var splitter = this.Meta.ReferenceViewInfo.SplitterIfMulti;
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

        /// <summary>
        /// 根据SelectedValuePath指定的值，获取目标属性值
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private object GetSelectedValue(Entity entity)
        {
            string selectedValuePath = this.Meta.ReferenceViewInfo.SelectedValuePath;
            if (string.IsNullOrEmpty(selectedValuePath)) { return entity.Id; }

            return entity.GetPropertyValue(selectedValuePath);
        }

        /// <summary>
        /// 当前是否处于多选的模式下。
        /// </summary>
        protected bool IsMultiSelection
        {
            get { return this.Meta.ReferenceViewInfo.SelectionMode == ReferenceSelectionMode.Multiple; }
        }
    }
}