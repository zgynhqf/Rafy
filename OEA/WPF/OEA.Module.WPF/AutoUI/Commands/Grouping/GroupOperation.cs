/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101125
 * 说明：为所有Command进行分组操作
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101125
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Itenso.Windows.Input;
using OEA.WPF.Command;
using OEA.MetaModel;
using OEA.MetaModel.View;

namespace OEA.Module.WPF.CommandAutoUI
{
    /// <summary>
    /// 职责：为所有Command进行分组操作
    /// </summary>
    class GroupOperation : CommandAutoUIComponent
    {
        /// <summary>
        /// 临时存放的所有生成的命令组
        /// </summary>
        private Dictionary<string, GeneratableCommandGroup> _tmpGroups;

        /// <summary>
        /// 把所有的命令进行分组，并为每个组创建对应的控件生成器
        /// </summary>
        /// <param name="commands"></param>
        /// <returns></returns>
        public GeneratableCommandGroup[] GroupCommands(IEnumerable<WPFCommand> commands)
        {
            var sortedCommands = SortAllCommands(commands);

            this._tmpGroups = new Dictionary<string, GeneratableCommandGroup>();

            foreach (var cmd in sortedCommands) { this.AddCommandIntoGroup(cmd); }

            return this._tmpGroups.Values.ToArray();
        }

        /// <summary>
        /// 把指定Command回到它对应的组中
        /// </summary>
        /// <param name="cmd"></param>
        private void AddCommandIntoGroup(WPFCommand cmd)
        {
            var algorithmType = cmd.GroupAlgorithmType ?? typeof(DefaultAlgorithm);

            var algorithm = Activator.CreateInstance(algorithmType) as GroupAlgorithm;
            algorithm.Context = this.Context;
            algorithm.FinalGroups = this._tmpGroups;
            algorithm.GroupCommand(cmd);
        }

        private static IEnumerable<WPFCommand> SortAllCommands(IEnumerable<WPFCommand> commands)
        {
            return commands;
            //var result = new List<WPFCommand>();
            //var gpCommands = commands.OrderBy(cmd => cmd.GroupType).GroupBy(cmd => cmd.GroupType);
            //foreach (var gpCommand in gpCommands)
            //{
            //    result.AddRange(new SortedSet<WPFCommand>(gpCommand.ToList(), new CommandBaseIndexComparer()));
            //}

            /////根据Index进行Command的排序
            //return result;
        }

        ///// <summary>
        ///// 根据Index进行Command的排序
        ///// </summary>
        //private class CommandBaseIndexComparer : IComparer<WPFCommand>
        //{
        //    public int Compare(WPFCommand x, WPFCommand y)
        //    {
        //        if (x.Index == 0 || x.Index == y.Index) return 1;
        //        else if (y.Index == 0) return -1;
        //        else
        //        {
        //            return x.Index.CompareTo(y.Index);
        //        }
        //    }
        //}
    }
}