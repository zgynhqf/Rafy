/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121118 19:39
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121118 19:39
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;

namespace Rafy
{
    /// <summary>
    /// bool 值的装箱值
    /// </summary>
    public static class BooleanBoxes
    {
        /// <summary>
        /// The true
        /// </summary>
        public static readonly object True = true;

        /// <summary>
        /// The false
        /// </summary>
        public static readonly object False = false;

        /// <summary>
        /// Boxes the specified bool value.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static object Box(this bool value)
        {
            return value ? True : False;
        }
    }
}
