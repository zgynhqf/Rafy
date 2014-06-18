/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101124
 * 说明：可生成控件的命令组
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101124
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.WPF.Command;

namespace Rafy.WPF.Command.UI
{
    /// <summary>
    /// 可生成控件的命令组
    /// 
    /// 附带了界面生成器的命令组
    /// </summary>
    public class GeneratableCommandGroup : CommandMetaGroup
    {
        public GeneratableCommandGroup(string name) : base(name) { }

        /// <summary>
        /// 附带的界面生成器
        /// </summary>
        public GroupGenerator Generator { get; set; }
    }
}
