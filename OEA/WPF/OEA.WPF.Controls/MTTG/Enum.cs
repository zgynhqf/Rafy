/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120829 21:05
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120829 21:05
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// CheckingRow 模式下的级联勾选行为模式
    /// </summary>
    public enum CheckingRowCascade
    {
        None = 0,

        /// <summary>
        /// 级联把父节点勾选上
        /// </summary>
        CascadeParent = 1,

        /// <summary>
        /// 级联把所有孩子节点勾选上
        /// </summary>
        CascadeChildren = 2
    }
}
