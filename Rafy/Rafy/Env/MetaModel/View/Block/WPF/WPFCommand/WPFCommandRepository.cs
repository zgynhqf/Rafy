/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：201101
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201101
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Rafy;
using Rafy.Reflection;
using Rafy.MetaModel.Attributes;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// 系统中所有 WPF 命令的仓库
    /// 
    /// 这里只存储这些命令的一个原始值。每个类型的视图 EVM 中的 Commands 是这些值的拷贝。
    /// </summary>
    public class WPFCommandRepository : MetaRepositoryBase<WPFCommand>
    {
        internal WPFCommandRepository() { }

        #region 查询 API

        public WPFCommand this[string commandName]
        {
            get { return this.Get(commandName); }
        }

        public WPFCommand Get(string commandName)
        {
            var jsCmd = this.Find(commandName);
            if (jsCmd == null)
            {
                var msg = string.Format("不存在客户端命令： {0} 。", commandName);
                throw new InvalidOperationException(msg);
            }
            return jsCmd;
        }

        public WPFCommand Find(string commandName)
        {
            return this.FirstOrDefault(c => c.Name == commandName);
        }

        public WPFCommand this[Type cmdType]
        {
            get { return this.Get(cmdType); }
        }

        public WPFCommand Get(Type cmdType)
        {
            var jsCmd = this.Find(cmdType);
            if (jsCmd == null)
            {
                var msg = string.Format("不存在客户端命令： {0} 。", cmdType.FullName);
                throw new InvalidOperationException(msg);
            }
            return jsCmd;
        }

        public WPFCommand Find(Type cmdType)
        {
            return this.FirstOrDefault(c => c.RuntimeType == cmdType);
        }

        #endregion

        internal void AddCommand(WPFCommand command)
        {
            this.AddPrime(command);
        }

        /// <summary>
        /// 添加程序集中 Commands 文件夹下的 js Resource。
        /// </summary>
        /// <param name="assembly"></param>
        public void AddByAssembly(Assembly assembly)
        {
            var commands = assembly.GetTypeMarked<CommandAttribute>();
            foreach (var cmd in commands)
            {
                var cmdMeta = this.Create(cmd);
                this.AddCommand(cmdMeta);
            }
        }

        /// <summary>
        /// 构造一个可变的元数据
        /// </summary>
        /// <param name="commandType"></param>
        /// <returns></returns>
        private WPFCommand Create(Type commandType)
        {
            WPFCommand result = null;

            var cmdAttri = commandType.GetSingleAttribute<CommandAttribute>();
            if (cmdAttri != null)
            {
                var commandInfoType = cmdAttri.CommandInfoType;
                if (commandInfoType != null) { result = Activator.CreateInstance(commandInfoType) as WPFCommand; }
            }

            if (result == null) { result = new WPFCommand(); }

            result.RuntimeType = commandType;

            if (cmdAttri != null)
            {
                result.Label = cmdAttri.Label ?? commandType.Name;
                result.ToolTip = cmdAttri.ToolTip;
                result.ImageName = cmdAttri.ImageName;
                result.GroupType = cmdAttri.GroupType;
                result.Location = cmdAttri.Location;
                result.GroupAlgorithmType = cmdAttri.UIAlgorithm;
                result.Gestures = cmdAttri.Gestures;
                var group = cmdAttri.Hierarchy ?? string.Empty;
                result._hierarchy = group.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            else
            {
                result.Label = commandType.Name;
            }

            return result;
        }
    }
}
