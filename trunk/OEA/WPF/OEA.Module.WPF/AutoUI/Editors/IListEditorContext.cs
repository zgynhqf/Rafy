/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110802
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110802
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using OEA.MetaModel;
using OEA.MetaModel.View;
using System.ComponentModel;
using OEA.Library;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 包含 ListEditor 所需要的环境数据。
    /// </summary>
    public interface IListEditorContext
    {
        EntityViewMeta Meta { get; }

        /// <summary>
        /// ListEditor 会向此对象报告它们的事件。
        /// </summary>
        IEventListener EventReporter { get; }

        /// <summary>
        /// 当前的数据列表
        /// </summary>
        EntityList Data { get; }
    }
}
