/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101124
 * 说明：命令界面子系统中的构件
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
using System.Diagnostics;

namespace OEA.Module.WPF.CommandAutoUI
{
    /// <summary>
    /// 命令界面子系统中的构件
    /// </summary>
    public abstract class CommandAutoUIComponent
    {
        /// <summary>
        /// 每个构件都需要其所在的上下文对象
        /// </summary>
        /// <param name="context"></param>
        public CommandAutoUIComponent(CommandAutoUIContext context)
        {
            this._context = context;
        }

        private CommandAutoUIContext _context;

        /// <summary>
        /// 构件所处的生成上下文对象
        /// </summary>
        public CommandAutoUIContext Context
        {
            get
            {
                Debug.Assert(this._context != null, "请先设置此属性");
                return this._context;
            }
        }
    }
}
