/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120928 13:57
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120928 13:57
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// bool 值的装箱值
    /// </summary>
    public static class BooleanBoxes
    {
        public static readonly object True = true;

        public static readonly object False = false;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static object Box(bool value)
        {
            return value ? True : False;
        }
    }
}
