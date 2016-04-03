/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100316
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100316
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// 命令生成位置
    /// </summary>
    [Flags]
    public enum CommandLocation
    {
        /// <summary>
        /// 默认为生成在 Toolbar 上，由一个按钮触发或者Group的MenuItem
        /// </summary>
        Toolbar = 1,

        /// <summary>
        /// 生成为菜单
        /// </summary>
        Menu = 2,

        All = Toolbar | Menu
    }
}
