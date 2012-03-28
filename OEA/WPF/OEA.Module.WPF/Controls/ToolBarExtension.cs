using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using OEA.Module.WPF.Controls;
using OEA.Command;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using OEA.MetaModel;
using OEA.MetaModel.View;

namespace OEA.Module.WPF
{
    public static class ToolBarExtension
    {
        /// <summary>
        /// 这个属性表示ToolBar是为这个Content控件服务的
        /// </summary>
        private static readonly DependencyProperty ToolBarServicedControlProperty =
            DependencyProperty.Register("ToolBarServicedControlProperty", typeof(FrameworkElement), typeof(ToolBarExtension));

        /// <summary>
        /// 这个属性表示ToolBar上所有已经添加的Command
        /// </summary>
        private static readonly DependencyProperty ToolBarCommandsProperty =
            DependencyProperty.Register("ToolBarCommandsProperty", typeof(Collection<WPFCommand>), typeof(ToolBarExtension));

        /// <summary>
        /// 这个属性表示ToolBar是为这个Content控件服务的
        /// </summary>
        /// <param name="commandsContainer"></param>
        /// <returns></returns>
        public static FrameworkElement GetServicedControl(this ItemsControl commandsContainer)
        {
            return (FrameworkElement)commandsContainer.GetValue(ToolBarServicedControlProperty);
        }

        /// <summary>
        /// 这个属性表示ToolBar是为这个Content控件服务的
        /// </summary>
        /// <param name="commandsContainer"></param>
        /// <param name="content"></param>
        public static void SetServicedControl(this ItemsControl commandsContainer, FrameworkElement content)
        {
            commandsContainer.SetValue(ToolBarServicedControlProperty, content);
        }

        /// <summary>
        /// 这个属性表示ToolBar上所有已经添加的Command
        /// </summary>
        /// <param name="commandsContainer"></param>
        /// <returns></returns>
        public static Collection<WPFCommand> GetAttachedCommands(this ItemsControl commandsContainer)
        {
            var commands = (Collection<WPFCommand>)commandsContainer.GetValue(ToolBarCommandsProperty);
            if (commands == null)
            {
                commands = new Collection<WPFCommand>();
                commandsContainer.SetValue(ToolBarCommandsProperty, commands);
            }
            return commands;
        }
    }
}
