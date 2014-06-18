/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110810
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110810
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Rafy.MetaModel;
using Rafy.Domain;
using Rafy.MetaModel.View;
using System.ComponentModel;

namespace Rafy.WPF
{
    /// <summary>
    /// 列表视图中的控件的 “Check选择” 模式
    /// </summary>
    public enum CheckingMode
    {
        /// <summary>
        /// 未开启 CheckBox 选择
        /// </summary>
        None,

        /// <summary>
        /// 使用 CheckBox 进行选择，并双向绑定到行的 IsChecked 属性上。
        /// </summary>
        CheckingRow
    }
}
