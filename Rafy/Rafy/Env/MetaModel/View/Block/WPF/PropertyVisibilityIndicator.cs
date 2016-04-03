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
using Rafy.ManagedProperty;
using Rafy.Utils;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 属性是否可见的指示器
    /// </summary>
    public class PropertyVisibilityIndicator : Freezable
    {
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

        private IManagedProperty _Property;
        /// <summary>
        /// 当状态为动态检查时，这个属性表示需要被检查的属性。
        /// 一个返回 bool 值的属性。
        /// </summary>
        public IManagedProperty Property
        {
            get { return this._Property; }
            set { this.SetValue(ref this._Property, value); }
        }
    }

    /// <summary>
    /// 可见性指示器的类型
    /// </summary>
    public enum VisiblityType
    {
        AlwaysShow,

        AlwaysHide,

        /// <summary>
        /// 动态表示是否需要检测动态属性来获取可见性
        /// </summary>
        Dynamic,
    }
}
