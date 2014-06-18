/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130514
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130514 11:50
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel.View;

namespace Rafy.WPF
{
    /// <summary>
    /// 列表视图可显示的位置
    /// </summary>
    public enum ListShowInWhere
    {
        /// <summary>
        /// 显示在列表中。
        /// </summary>
        List = ShowInWhere.List,
        /// <summary>
        /// 显示在下拉框中。
        /// </summary>
        DropDown = ShowInWhere.DropDown
    }
}