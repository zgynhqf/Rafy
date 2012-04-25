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
using OEA.ManagedProperty;

namespace OEA.MetaModel
{
    /// <summary>
    /// 属性是否可见的指示器
    /// </summary>
    public class PropertyReadonlyIndicator : Freezable
    {
        private PropertyReadonlyStatus _Status;
        /// <summary>
        /// 属性只读检测的状态
        /// </summary>
        public PropertyReadonlyStatus Status
        {
            get { return this._Status; }
            set { this.SetValue(ref this._Status, value); }
        }

        private IManagedProperty _Property;
        /// <summary>
        /// 当状态为动态检查时，这个属性表示需要被检查的属性。
        /// </summary>
        public IManagedProperty Property
        {
            get { return this._Property; }
            set { this.SetValue(ref this._Property, value); }
        }
    }

    /// <summary>
    /// 属性只读检测的状态
    /// </summary>
    public enum PropertyReadonlyStatus
    {
        /// <summary>
        /// 未进行设置
        /// </summary>
        None,

        /// <summary>
        /// 必然是只读属性
        /// </summary>
        Readonly,

        /// <summary>
        /// 需要动态检查某个属性来获取“是否只读”
        /// </summary>
        Dynamic
    }
}
