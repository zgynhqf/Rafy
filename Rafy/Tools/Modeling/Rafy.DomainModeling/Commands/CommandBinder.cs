/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130412 15:32
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130412 15:32
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Rafy.DomainModeling.Controls;

namespace Rafy.DomainModeling.Commands
{
    /// <summary>
    /// 执行命令绑定方法的类型。
    /// </summary>
    public static class CommandBinder
    {
        /// <summary>
        /// 为设计器绑定指定的命令到指定的元素中。
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="designer"></param>
        /// <param name="context">如果没有指定上下文，则使用 designer 作为上下文。</param>
        public static void Bind<TCommand>(ModelingDesigner designer, UIElement context = null)
            where TCommand : WPFDesignerCommand, new()
        {
            var command = new TCommand();
            if (context == null) context = designer;

            var binding = WPFCommandAdapter.CreateCommandBinding(designer, command);

            context.CommandBindings.Add(binding);
        }

        /// <summary>
        /// 移除指定的 WPF 命令。
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="designer"></param>
        public static void Unbind<TCommand>(UIElement context)
            where TCommand : WPFDesignerCommand, new()
        {
            var command = new TCommand().GetWPFCommand();

            var bindings = context.CommandBindings;
            for (int i = 0, c = bindings.Count; i < c; i++)
            {
                var binding = bindings[i];
                if (binding.Command == command)
                {
                    bindings.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
