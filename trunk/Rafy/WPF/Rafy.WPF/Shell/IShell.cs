/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110509
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110509
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.WPF
{
    /// <summary>
    /// 程序外壳。
    /// </summary>
    public interface IShell
    {
        /// <summary>
        /// 主工作区。
        /// </summary>
        IWorkspace Workspace { get; }
    }
}
