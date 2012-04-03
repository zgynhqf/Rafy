/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101124
 * 说明：一组命令的界面生成器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101124
 * 
*******************************************************/

using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Itenso.Windows.Input;

using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.WPF.Command;


namespace OEA.Module.WPF.CommandAutoUI
{
    /// <summary>
    /// 一组命令的界面生成器
    /// 
    /// 目前分配了两个职责：
    /// 1. 动态为命令生成控件。
    /// 2. 使用动态算法将控件添加到上下文对象中
    /// </summary>
    public abstract class GroupGenerator : CommandAutoUIComponent
    {
        private CommandGroup _commandGroup;

        public GroupGenerator(CommandGroup group, CommandAutoUIContext context)
            : base(context)
        {
            this._commandGroup = group;
        }

        /// <summary>
        /// 为这个命令组生成控件
        /// </summary>
        protected CommandGroup CommandGroup
        {
            get { return this._commandGroup; }
        }

        /// <summary>
        /// 使用动态算法将控件添加到上下文对象中。
        /// </summary>
        /// <param name="control"></param>
        public virtual void CreateControlToContext()
        {
            var control = this.CreateControlCore();
            if (control == null) return;

            this.AttachToContextCore(control);
        }

        /// <summary>
        /// 子类完成此方法实现具体的生成控件方法。
        /// </summary>
        /// <returns></returns>
        protected virtual FrameworkElement CreateControlCore() { return null; }

        /// <summary>
        /// 子类完成此方法实现具体的生成控件方法。
        /// </summary>
        /// <param name="control"></param>
        protected virtual void AttachToContextCore(FrameworkElement control)
        {
            this.AttachToToolbar(control);
        }

        #region 其它API

        /// <summary>
        /// 由于本类的职责有两个：生成控件、附加到环境中。所以此API预留给此需要生成控件的场景。
        /// </summary>
        /// <returns></returns>
        public FrameworkElement CreateControl()
        {
            return this.CreateControlCore();
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 添加到右键菜单
        /// </summary>
        /// <param name="control"></param>
        protected virtual void AttachToContextMenu(FrameworkElement control)
        {
            var contextMenu = this.FindOrCreateContextMenu();
            contextMenu.Items.Add(control);
        }

        protected ContextMenu FindOrCreateContextMenu()
        {
            var mainContentControl = this.Context.CommandsContainer.GetServicedControl();
            var contextMenu = mainContentControl.ContextMenu;
            if (contextMenu == null)
            {
                contextMenu = new ContextMenu();
                mainContentControl.ContextMenu = contextMenu;
            }
            return contextMenu;
        }

        /// <summary>
        /// 简单的添加到Toolbar中
        /// </summary>
        /// <param name="control"></param>
        protected virtual void AttachToToolbar(FrameworkElement control)
        {
            this.Context.Items.Add(control);
        }

        /// <summary>
        /// 尝试执行某命令
        /// </summary>
        /// <param name="cmdSource">
        /// 命令附加在这个命令源上。
        /// </param>
        protected static void TryExcuteCommand(ICommandSource cmdSource)
        {
            var cmd = (cmdSource.Command as CommandAdapter).CoreCommand;
            CommandRepository.TryExecuteCommand(cmd, cmdSource.CommandParameter);
        }

        protected void TryExcuteCommand(IClientCommand runtimeCmd)
        {
            CommandRepository.TryExecuteCommand(runtimeCmd, this.Context.CommandArg);
        }

        /// <summary>
        /// 为运行时的Command生成一个按钮
        /// </summary>
        /// <param name="runtimeCommand"></param>
        /// <returns></returns>
        protected Button CreateAButton(CommandAdapter runtimeCommand)
        {
            var btn = new Button();

            btn.Name = "btn" + runtimeCommand.CoreCommand.ProgramingName;
            btn.CommandParameter = this.Context.CommandArg;
            ButtonCommand.SetCommand(btn, runtimeCommand);

            return btn;
        }

        /// <summary>
        /// 为运行时的Command生成一个菜单项
        /// </summary>
        /// <param name="runtimeCommand"></param>
        /// <returns></returns>
        protected MenuItem CreateAMenuItem(CommandAdapter runtimeCommand)
        {
            MenuItem menuItem = new MenuItem();
            menuItem.CommandParameter = this.Context.CommandArg;
            MenuItemCommand.SetCommand(menuItem, runtimeCommand);

            return menuItem;
        }

        /// <summary>
        /// 为一个运行时的Command生成TextBox控件。
        /// </summary>
        /// <param name="runtimeCommand"></param>
        /// <returns></returns>
        protected static TextBox CreateTextBox(ClientCommand runtimeCommand)
        {
            var textBox = new TipTextBox()
            {
                Name = runtimeCommand.ProgramingName,
                Width = 150,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(2),
                EmptyValue = runtimeCommand.CommandInfo.ToolTip
            };

            //当TextBox的值改变时，通知命令进行新的输入值
            textBox.TextChanged += (o, e) =>
            {
                runtimeCommand.SetCustomParams(CommandCustomParams.TextBox, textBox.Text);
            };

            //支持UI Test
            AutomationProperties.SetName(textBox, runtimeCommand.Label);

            return textBox;
        }

        #endregion
    }
}