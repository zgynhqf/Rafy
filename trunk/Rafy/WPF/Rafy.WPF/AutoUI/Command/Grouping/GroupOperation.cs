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
using Rafy.WPF.Command;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Rafy.WPF.Command.UI
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
        internal GeneratableCommandGroup[] GroupCommands()
        {
            this._tmpGroups = new Dictionary<string, GeneratableCommandGroup>();

            foreach (var cmd in this.Context.Commands) { this.AddCommandIntoGroup(cmd); }

            return this._tmpGroups.Values.ToArray();
        }

        /// <summary>
        /// 使用元数据中指定的分组算法，将命令进行分组。
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
    }
}