/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130403 16:22
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130403 16:22
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
    /// 枚举项。
    /// </summary>
    public class EnumItem : FrameworkElement
    {
        #region Name DependencyProperty

        public static readonly DependencyProperty ItemNameProperty = DependencyProperty.Register(
            "ItemName", typeof(string), typeof(EnumItem)
            );

        /// <summary>
        /// 枚举项名称
        /// </summary>
        public string ItemName
        {
            get { return (string)this.GetValue(ItemNameProperty); }
            set { this.SetValue(ItemNameProperty, value); }
        }

        #endregion

        #region Label DependencyProperty

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof(string), typeof(EnumItem)
            );

        /// <summary>
        /// 注释/业务名
        /// </summary>
        public string Label
        {
            get { return (string)this.GetValue(LabelProperty); }
            set { this.SetValue(LabelProperty, value); }
        }

        #endregion

        //#region Value DependencyProperty

        //public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        //    "Value", typeof(int), typeof(EnumItem)
        //    );

        ///// <summary>
        ///// 对应的数据值。
        ///// </summary>
        //public int Value
        //{
        //    get { return (int)this.GetValue(ValueProperty); }
        //    set { this.SetValue(ValueProperty, value); }
        //}

        //#endregion

        private string Display
        {
            get
            {
                var value = this.ItemName;
                if (Label != null)
                {
                    value += "(" + Label + ")";
                }
                return value;
            }
        }

        public override string ToString()
        {
            return this.Display;
        }
    }
}