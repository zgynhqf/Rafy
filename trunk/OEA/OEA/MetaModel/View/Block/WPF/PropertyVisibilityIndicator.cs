/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110316
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100316
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel
{
    /// <summary>
    /// 属性是否可见的指示器
    /// </summary>
    public class PropertyVisibilityIndicator : Freezable
    {
        private string _PropertyName;

        private VisiblityType _VisiblityType;

        /// <summary>
        /// 可见性指示器的类型
        /// </summary>
        public VisiblityType VisiblityType
        {
            get { return this._VisiblityType; }
            set { this.SetValue(ref this._VisiblityType, value); }
        }

        /// <summary>
        /// 是否需要检测动态属性来获取可见性
        /// </summary>
        public bool IsDynamic
        {
            get { return this._VisiblityType == VisiblityType.Dynamic; }
        }

        /// <summary>
        /// 一个返回 bool 值的属性名。
        /// </summary>
        public string PropertyName
        {
            get { return this._PropertyName; }
            set { this.SetValue(ref this._PropertyName, value); }
        }
    }
}
