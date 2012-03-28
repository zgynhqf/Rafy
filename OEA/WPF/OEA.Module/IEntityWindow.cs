/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110218
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100218
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleCsla.Core;

using OEA.MetaModel;
using OEA.Library;
using OEA.Module;

namespace OEA.Module
{
    /// <summary>
    /// 实体窗口模板
    /// </summary>
    public interface IEntityWindow : IWorkspaceWindow
    {
        /// <summary>
        /// 对应的窗口主要的view
        /// 
        /// 如果是用户自定义的模块，该属性可以为null
        /// </summary>
        ObjectView View { get; }
    }
}