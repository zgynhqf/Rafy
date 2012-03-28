/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111202
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111202
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace OEA.UIA.Utils
{
    /// <summary>
    /// 为了使 API 对 OEA 下拉列表和系统的下拉框分辨开，这里定义了一个 WpfControl 的包装器。
    /// </summary>
    public class WpfComboListControl
    {
        internal WpfComboBox Control;
    }
}
