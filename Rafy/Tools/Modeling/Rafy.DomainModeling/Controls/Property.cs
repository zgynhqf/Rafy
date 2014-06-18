/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130403 16:30
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130403 16:30
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Rafy.DomainModeling.Controls
{
    /// <summary>
    /// 值属性
    /// </summary>
    public class Property : FrameworkElement
    {
        #region Name DependencyProperty

        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register(
            "PropertyName", typeof(string), typeof(Property)
            );

        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName
        {
            get { return (string)this.GetValue(PropertyNameProperty); }
            set { this.SetValue(PropertyNameProperty, value); }
        }

        #endregion

        #region PropertyType DependencyProperty

        public static readonly DependencyProperty PropertyTypeProperty = DependencyProperty.Register(
            "PropertyType", typeof(string), typeof(Property)
            );

        /// <summary>
        /// 属性类型
        /// </summary>
        public string PropertyType
        {
            get { return (string)this.GetValue(PropertyTypeProperty); }
            set { this.SetValue(PropertyTypeProperty, value); }
        }

        #endregion

        #region Label DependencyProperty

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof(string), typeof(Property)
            );

        /// <summary>
        /// 属性标签
        /// </summary>
        public string Label
        {
            get { return (string)this.GetValue(LabelProperty); }
            set { this.SetValue(LabelProperty, value); }
        }

        #endregion

        public string Display
        {
            get
            {
                var value = string.Format("{0} {1}", this.PropertyType, this.PropertyName);
                if (Label != null)
                {
                    value += "(" + Label + ")";
                }
                return value;
            }
        }
    }
}