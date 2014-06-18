/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101124
 * 说明：命令UI生成子系统的入口点
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101124
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Rafy.WPF.Command;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Rafy.WPF.Command.UI
{
    /// <summary>
    /// 命令UI生成子系统的入口点
    /// </summary>
    internal class CommandAutoUIManager
    {
        /// <summary>
        /// 为指定的Toolbar生成子项。
        /// </summary>
        /// <param name="commandsContainer">需要生成Commands的toolbar</param>
        /// <param name="boType">toolbar是为这个类型提供服务</param>
        /// <param name="commandArg"></param>
        /// <param name="commands">在这个command集合中筛选</param>
        public void Generate(ItemsControl commandsContainer, object commandArg, IEnumerable<WPFCommand> commands)
        {
            if (commandsContainer == null) throw new ArgumentNullException("toolbar");
            if (commands == null) throw new ArgumentNullException("commands");

            //生成一个上下文对象
            var context = new CommandAutoUIContext(commandsContainer, commandArg, commands);

            ////过滤不可用的命令
            //var filteredCommands = this.FilterExsitingCommands(commandsContainer, commands);

            DoGenerate(context);

            ResetContainerVisibility(commandsContainer);

            SealViewCommands(commandArg);
        }

        /// <summary>
        /// 执行生成命令的核心逻辑。
        /// </summary>
        /// <param name="context"></param>
        private static void DoGenerate(CommandAutoUIContext context)
        {
            //使用分组算法进行分组，并为每个组创建对应的控件生成器
            //注意：如果是单独的Command，一样生成一个单独的“组”
            var grouping = new GroupOperation { Context = context };
            var groups = grouping.GroupCommands();

            //对于每一个命令组，开始生成控件，并添加到上下文对象中。
            foreach (var group in groups)
            {
                var generator = group.Generator;
                generator.CreateControlToContext();
            }

            //根据分组生成的控件，也都加入到容器中。
            context.AttachGroupedItems();
        }

        private static void ResetContainerVisibility(ItemsControl commandsContainer)
        {
            //如果没有生成任何一个项，则这个toolbar里面设置为不可见。
            if (commandsContainer.Items.Count == 0)
            {
                commandsContainer.Visibility = Visibility.Collapsed;
            }
            //如果之前设置为不可见了，则这里需要令它显示。
            else if (commandsContainer.Items.Count > 0 && commandsContainer.Visibility == Visibility.Collapsed)
            {
                commandsContainer.Visibility = Visibility.Visible;
            }
        }

        private static void SealViewCommands(object commandArg)
        {
            var view = commandArg as LogicalView;
            if (view != null) { view.Commands.Seal(); }
        }

        //可以添加重复的命令
        ///// <summary>
        ///// 过滤掉重复的命令
        ///// </summary>
        ///// <param name="commandsContainer"></param>
        ///// <param name="commands"></param>
        ///// <returns></returns>
        //private List<WPFCommand> FilterExsitingCommands(ItemsControl commandsContainer, IEnumerable<WPFCommand> commands)
        //{
        //    var toolbarCommands = commandsContainer.GetAttachedCommands();
        //    var result = new List<WPFCommand>();
        //    foreach (var command in commands)
        //    {
        //        //过滤相同的command
        //        if (toolbarCommands.Any(commandAdded => commandAdded.Name == command.Name)) continue;

        //        toolbarCommands.Add(command);
        //        result.Add(command);
        //    }

        //    return result;
        //}
    }
}