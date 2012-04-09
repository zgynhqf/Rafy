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

            var property = this.PropertyViewInfo;
            if (string.IsNullOrEmpty(property.ReferenceViewInfo.RefEntityProperty))
            {
                bindingPath = property.Name;
                if (this.PropertyCanWrite) { bindingMode = BindingMode.TwoWay; }
            }
            else
            {
                bindingPath = this.PropertyViewInfo.BindingPath();
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
        protected void NotifyReferenceSelected(Entity selecteEntity)
        {
            if (selecteEntity != null)
            {
                var curObj = this.Context.CurrentObject;
                if (curObj != null)
                {
                    var refInfo = this.PropertyViewInfo.ReferenceViewInfo;
                    if (!string.IsNullOrEmpty(refInfo.RefEntityProperty))
                    {
                        this.OnReferenceEntityChanging();
                        curObj.SetPropertyValue(refInfo.RefEntityProperty, selecteEntity);
                    }
                }

                //赋值给this.PropertyValue
                this.PropertyValue = this.GetSelectedValue(selecteEntity);
            }
        }

        /// <summary>
        /// 根据SelectedValuePath指定的值，获取目标属性值
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected object GetSelectedValue(Entity entity)
        {
            string selectedValuePath = this.PropertyViewInfo.ReferenceViewInfo.SelectedValuePath;
            if (string.IsNullOrEmpty(selectedValuePath)) { return entity.Id; }

            return entity.GetPropertyValue(selectedValuePath);
        }
    }
}