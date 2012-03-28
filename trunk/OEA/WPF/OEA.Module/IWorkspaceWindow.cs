/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110426
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110426
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.Module
{
    /// <summary>
    /// 在 Workspace 中显示的每一个“页签”
    /// </summary>
    public interface IWorkspaceWindow
    {
        /// <summary>
        /// 窗口的标题。（唯一确定窗体的标识）
        /// </summary>
        string Title { get; }
    }
}
