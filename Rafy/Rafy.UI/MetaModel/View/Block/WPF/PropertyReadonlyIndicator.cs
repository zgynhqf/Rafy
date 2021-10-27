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
    public class PropertyReadonlyIndicator : Freezable
    {
        private ReadOnlyStatus _Status;
        /// <summary>
        /// 属性只读检测的状态
        /// </summary>
        public ReadOnlyStatus Status
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
    /// 三态的只读性。
    /// </summary>
    public enum ReadOnlyStatus
    {
        /// <summary>
        /// 可编辑状态。
        /// </summary>
        None,
        /// <summary>
        /// 只读状态。
        /// </summary>
        ReadOnly,
        /// <summary>
        /// 则表示不强制只读，容器按照自己相应的规则来计算自己的只读性。
        /// </summary>
        Dynamic
    }
}
