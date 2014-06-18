/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110217
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110217
 * 
*******************************************************/

using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF.Editors;
using Rafy.Reflection;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 树型表的列的基类
    /// </summary>
    public abstract class TreeColumn : TreeGridColumn
    {
        #region 字段

        /// <summary>
        /// 该字段可能为空，表示该列没有对应任何元数据。
        /// </summary>
        private WPFEntityPropertyViewMeta _meta;

        #endregion

        /// <summary>
        /// 本列对应的编辑器
        /// 
        /// 不会为 null。
        /// </summary>
        public PropertyEditor Editor { get; internal set; }

        /// <summary>
        /// 正在编辑的属性
        /// </summary>
        public WPFEntityPropertyViewMeta Meta
        {
            get { return this._meta; }
            internal set { this._meta = value; }
        }

        #region 编辑状态

        /// <summary>
        /// 根据实体当前的状态，来计算这个实体在本列上是否可以编辑。
        /// </summary>
        /// <param name="dataItem"></param>
        /// <returns></returns>
        protected override bool CanEnterEditing(object dataItem)
        {
            return !ReadOnlyMatrix.CheckIsReadOnly(dataItem as Entity, this.Meta);
        }

        internal protected override void PrepareElementForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            this.Editor.PrepareElementForEdit(editingElement, editingEventArgs);
        }

        #endregion

        /// <summary>
        /// 生成这个Column使用的编辑Element
        /// 未实现
        /// </summary>
        /// <returns></returns>
        protected override FrameworkElement GenerateEditingElementCore()
        {
            return this.Editor.Control;
        }

        internal void UpdateVisibility(Entity currData)
        {
            if (this._meta != null)
            {
                bool isVisible = false;
                var visibilityIndicator = this._meta.VisibilityIndicator;

                //如果是动态计算，则尝试从数据中获取是否可见的值。
                if (visibilityIndicator.IsDynamic)
                {
                    if (currData != null)
                    {
                        isVisible = (bool)currData.GetProperty(visibilityIndicator.Property);
                    }
                    else
                    {
                        isVisible = true;
                    }
                }
                else
                {
                    isVisible = visibilityIndicator.VisiblityType == VisiblityType.AlwaysShow;
                }

                this.IsVisible = isVisible;
            }
        }

        #region 汇总

        internal protected override bool NeedSummary
        {
            get
            {
                if (this._meta == null) { return false; }

                var metaValue = GetNeedSummary(this._meta);
                if (metaValue.HasValue) return metaValue.Value;

                //整形、浮点型都可以进行合计。
                return TypeHelper.IsNumber(this._meta.PropertyMeta.ManagedProperty.PropertyType);
            }
        }

        internal protected override string GetSummary(ItemCollection items)
        {
            var mp = this._meta.PropertyMeta.ManagedProperty;

            var sum = items.Cast<object>().Sum(item =>
            {
                var e = item as Entity;
                var value = e.GetProperty(mp);
                return Convert.ToDouble(value);
            });

            return sum.ToString();
        }

        #region WPFEntityPropertyViewMeta.NeedSummary

        private const string PropertyMetaNeedSummary = "TreeColumn_PropertyMetaNeedSummary";

        public static bool? GetNeedSummary(WPFEntityPropertyViewMeta meta)
        {
            return meta.GetPropertyOrDefault<bool?>(PropertyMetaNeedSummary);
        }

        internal static void SetNeedSummary(WPFEntityPropertyViewMeta meta, bool? value)
        {
            meta.SetExtendedProperty(PropertyMetaNeedSummary, value);
        }

        #endregion

        #endregion
    }
}