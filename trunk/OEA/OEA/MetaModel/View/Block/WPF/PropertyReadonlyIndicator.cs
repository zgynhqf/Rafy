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
    public class PropertyReadonlyIndicator : Freezable
    {
        private PropertyReadonlyStatus _Status;

        public PropertyReadonlyStatus Status
        {
            get
            {
                return this._Status;
            }
            set
            {
                this.SetValue(ref this._Status, value);
            }
        }

        private string _PropertyName;

        public string PropertyName
        {
            get
            {
                return this._PropertyName;
            }
            set
            {
                this.SetValue(ref this._PropertyName, value);
            }
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
