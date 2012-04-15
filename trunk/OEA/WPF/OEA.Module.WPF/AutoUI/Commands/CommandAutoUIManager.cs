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
using Itenso.Windows.Input;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.WPF.Command;
using Wpf.Controls;

namespace OEA.Module.WPF.CommandAutoUI
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
            var context = new CommandAutoUIContext(commandsContainer, commandArg);

            ////过滤不可用的命令
            //var filteredCommands = this.FilterExsitingCommands(commandsContainer, commands);

            //使用分组算法进行分组，并为每个组创建对应的控件生成器
            //注意：如果是单独的Command，一样生成一个单独的“组”
            var grouping = new GroupOperation(context);
            var groups = grouping.GroupCommands(commands);

            //对于每一个命令组，开始生成控件，并添加到上下文对象中。
            foreach (var group in groups)
            {
                var generator = group.Generator;
                generator.CreateControlToContext();
            }

            //根据分组类型重新重新生成控件
            context.AttachGroupedItems();

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

            var view = commandArg as ObjectView;
            if (view != null) { view.Commands.Seal(); }
        }

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
        //        if (toolbarCommands.Any(commandAdded => commandAdded.Id == command.Id)) continue;

        //        toolbarCommands.Add(command);
        //        result.Add(command);
        //    }

        //    return result;
        //}
    }
}