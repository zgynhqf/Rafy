/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101124
 * 说明：代表一组命令
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
using OEA.Command;
using System.Diagnostics;
using OEA.MetaModel;
using OEA.MetaModel.View;

namespace OEA.Module.WPF.CommandAutoUI
{
    /// <summary>
    /// 代表一组命令，包含所有命令的元数据
    /// </summary>
    [DebuggerDisplay("{Name} : {Commands.Count}")]
    public class CommandGroup
    {
        public CommandGroup(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// 组名
        /// </summary>
        public string Name { get; private set; }

        private List<WPFCommand> _commands;

        /// <summary>
        /// 本组中的所有Command
        /// </summary>
        public IList<WPFCommand> Commands
        {
            get
            {
                if (_commands == null)
                {
                    _commands = new List<WPFCommand>();
                    //_commands = new SortedSet<OEA.Command.CommandBase>(new CommandBaseCompare());
                }

                return _commands;
            }
        }

        /// <summary>
        /// 添加一个Command
        /// </summary>
        /// <param name="item"></param>
        public void AddCommand(WPFCommand item)
        {
            this.Commands.Add(item);
        }

        /// <summary>
        /// 是否本组只有一个Command
        /// </summary>
        public bool HasSingleItem
        {
            get
            {
                return this.Commands.Count == 1;
            }
        }

        /// <summary>
        /// 如果本组只有一个Command，则返回唯一的Command
        /// </summary>
        public WPFCommand SingleItem
        {
            get
            {
                if (this.HasSingleItem)
                {
                    return this.Commands[0];
                }

                return null;
            }
        }
    }
}