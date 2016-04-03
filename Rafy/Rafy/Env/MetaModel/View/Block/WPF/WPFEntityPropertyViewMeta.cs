/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130903
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130903 17:35
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;

namespace Rafy.MetaModel.View
{
    public class WPFEntityPropertyViewMeta : EntityPropertyViewMeta
    {
        private PropertyReadonlyIndicator _ReadonlyIndicator = new PropertyReadonlyIndicator();
        /// <summary>
        /// 用于检测是否只读的属性
        /// </summary>
        public PropertyReadonlyIndicator ReadonlyIndicator
        {
            get { return this._ReadonlyIndicator; }
            set { this.SetValue(ref this._ReadonlyIndicator, value); }
        }

        private string _StringFormat;
        /// <summary>
        /// 格式化String Column Format
        /// </summary>
        public string StringFormat
        {
            get { return this._StringFormat; }
            set { this.SetValue(ref this._StringFormat, value); }
        }

        private string _EditorName;
        /// <summary>
        /// 编辑器名
        /// EditorAttribute中标记
        /// </summary>
        public string EditorName
        {
            get { return this._EditorName; }
            set { this.SetValue(ref this._EditorName, value); }
        }

        private double? _GridWidth;
        /// <summary>
        /// 用于初始化表格控件的宽度属性
        /// </summary>
        public double? GridWidth
        {
            get { return this._GridWidth; }
            set { this.SetValue(ref this._GridWidth, value); }
        }

        private int? _DetailColumnsSpan;
        /// <summary>
        /// 表单中该属性所占的列数。
        /// 
        /// 只在 DetailLayoutMode.AutoGrid 模式下有用。
        /// </summary>
        public int? DetailColumnsSpan
        {
            get { return this._DetailColumnsSpan; }
            set { this.SetValue(ref this._DetailColumnsSpan, value); }
        }

        private double? _DetailContentWidth;
        /// <summary>
        /// 表单中该属性所占的格子宽度。
        /// 
        /// 如果值在 0 到 1 之间，表示百分比，只有 DetailLayoutMode.AutoGrid 模式下可用。
        /// 否则表示绝对值。
        /// 
        /// 不指定，则使用系统默认值。
        /// </summary>
        public double? DetailContentWidth
        {
            get { return this._DetailContentWidth; }
            set { this.SetValue(ref this._DetailContentWidth, value); }
        }

        private double? _DetailHeight;
        /// <summary>
        /// 表单中该属性所占的总高度
        /// 不指定，则使用系统默认宽度。
        /// </summary>
        public double? DetailHeight
        {
            get { return this._DetailHeight; }
            set { this.SetValue(ref this._DetailHeight, value); }
        }

        private double? _DetailLabelSize;
        /// <summary>
        /// 在 DetailPanel 中显示的 Label 的宽度或者高度。
        /// 不指定，则使用系统默认值。
        /// </summary>
        public double? DetailLabelSize
        {
            get { return this._DetailLabelSize; }
            set { this.SetValue(ref this._DetailLabelSize, value); }
        }

        private bool _DetailNewLine;
        /// <summary>
        /// 指定某个属性在表单中是否需要开启新行。
        /// 
        /// 此属性只在 DetailLayoutMode.Wrapping 下有用。
        /// </summary>
        public bool DetailNewLine
        {
            get { return this._DetailNewLine; }
            set { this.SetValue(ref this._DetailNewLine, value); }
        }

        private bool? _DetailAsHorizontal;
        /// <summary>
        /// 在 DetailPanel 是否横向布局。
        /// </summary>
        public bool? DetailAsHorizontal
        {
            get { return this._DetailAsHorizontal; }
            set { this.SetValue(ref this._DetailAsHorizontal, value); }
        }

        private IManagedProperty _DisplayDelegate;
        /// <summary>
        /// 如果当前对象是一个引用属性，则可以指定一个额外的属性来进行代理显示。
        /// </summary>
        public IManagedProperty DisplayDelegate
        {
            get { return this._DisplayDelegate; }
            set { this.SetValue(ref this._DisplayDelegate, value); }
        }

        private NavigationPropertyMeta _NavigationPropertyMeta;
        /// <summary>
        /// 如果当前属性是一个导航触发属性，则这个属性不为 null。
        /// </summary>
        public NavigationPropertyMeta NavigationMeta
        {
            get { return this._NavigationPropertyMeta; }
            set { this.SetValue(ref this._NavigationPropertyMeta, value); }
        }

        #region 查询方法

        /// <summary>
        /// 用于绑定显示的属性名
        /// </summary>
        /// <returns></returns>
        public string DisplayPath()
        {
            return string.Join(".", this.DisplayPathProperties().Select(p => p.Name));
        }

        /// <summary>
        /// 用于绑定显示的属性名
        /// 
        /// 三种情况：
        /// 1.当前属性。
        /// 2.当前引用属性.名称。（如果显示属性为两个，那么第一个一定是引用属性，而第二个则是引用实体上的属性。）
        /// 3.显示代理属性。
        /// </summary>
        /// <returns></returns>
        public List<IManagedProperty> DisplayPathProperties()
        {
            var properties = new List<IManagedProperty>(2);

            if (this.IsReference)
            {
                if (this.DisplayDelegate != null)
                {
                    properties.Add(this.DisplayDelegate);
                }
                else
                {
                    var svm = this.SelectionViewMeta;

                    //该引用属性没有对应的引用实体属性
                    var title = svm.RefTypeDefaultView.TitleProperty;
                    if (title == null)
                    {
                        properties.Add(this.PropertyMeta.ManagedProperty);
                    }
                    else
                    {
                        properties.Add(this.PropertyMeta.ManagedProperty);
                        properties.Add(title.PropertyMeta.ManagedProperty);
                    }
                }
            }
            else
            {
                properties.Add(this.PropertyMeta.ManagedProperty);
            }

            return properties;
        }

        #endregion
    }
}
