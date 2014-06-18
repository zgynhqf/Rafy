/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110616
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110616
 * 创建文件 胡庆访 20120914
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using System.Windows;
using System.Windows.Data;

namespace Rafy.WPF.Editors
{
    internal class ReadOnlyMatrix
    {
        /// <summary>
        /// 检测某个实体对象的某个实体属性是否可以只读。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        internal static bool CheckIsReadOnly(Entity entity, WPFEntityPropertyViewMeta property)
        {
            //类指明为只读
            var indicator = property.ReadonlyIndicator;
            if (indicator.Status == ReadOnlyStatus.ReadOnly || property.Owner.NotAllowEdit) { return true; }

            //检测动态属性
            if (indicator.Status == ReadOnlyStatus.Dynamic && entity != null)
            {
                return (bool)entity.GetProperty(indicator.Property);
            }

            return false;
        }

        private ReadOnlyStatus _isReadOnly = ReadOnlyStatus.Dynamic;

        internal FrameworkElement Element;

        internal DependencyProperty ReadOnlyProperty;

        internal WPFEntityPropertyViewMeta Meta;

        internal bool ReadOnlyValue;

        /// <summary>
        /// 设置初始的 IsReadOnly 的值。
        /// </summary>
        internal void SetInitVallue()
        {
            var indicator = this.Meta.ReadonlyIndicator;
            switch (indicator.Status)
            {
                case ReadOnlyStatus.None:
                case ReadOnlyStatus.ReadOnly:
                    this.IsReadOnly = indicator.Status;
                    break;
                case ReadOnlyStatus.Dynamic:
                    this.BindDymanicReadOnly();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 只读性
        /// </summary>
        public ReadOnlyStatus IsReadOnly
        {
            get { return this._isReadOnly; }
            set
            {
                if (this._isReadOnly != value)
                {
                    this._isReadOnly = value;

                    if (value != ReadOnlyStatus.Dynamic)
                    {
                        this.Element.SetValue(this.ReadOnlyProperty, value == ReadOnlyStatus.ReadOnly ? this.ReadOnlyValue : !this.ReadOnlyValue);
                    }
                    else
                    {
                        //当设置 IsReadOnly 为 null 时，需要通过指示器状态，来设置元素的属性。
                        var indicator = this.Meta.ReadonlyIndicator;
                        switch (indicator.Status)
                        {
                            case ReadOnlyStatus.None:
                                this.Element.SetValue(this.ReadOnlyProperty, !this.ReadOnlyValue);
                                break;
                            case ReadOnlyStatus.ReadOnly:
                                this.Element.SetValue(this.ReadOnlyProperty, this.ReadOnlyValue);
                                break;
                            case ReadOnlyStatus.Dynamic:
                                this.Element.ClearValue(this.ReadOnlyProperty);
                                this.BindDymanicReadOnly();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 在动态只读的情况下，获取当前的只读性。
        /// </summary>
        /// <returns></returns>
        public bool GetRuntimeReadOnly()
        {
            var value = (bool)this.Element.GetValue(this.ReadOnlyProperty);
            return value == this.ReadOnlyValue;
        }

        private void BindDymanicReadOnly()
        {
            //绑定动态属性
            var indicator = this.Meta.ReadonlyIndicator;
            var rb = new Binding(indicator.Property.Name);
            rb.Mode = BindingMode.OneWay;

            if (!this.ReadOnlyValue) rb.Converter = new ReverseBooleanConverter();

            this.Element.SetBinding(this.ReadOnlyProperty, rb);
        }
    }
}