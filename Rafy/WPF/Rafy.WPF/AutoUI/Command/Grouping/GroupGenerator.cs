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
using Rafy.WPF.Command;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using System.Text.RegularExpressions;

namespace Rafy.WPF.Command.UI
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
        /// <summary>
        /// 为这个命令组生成控件
        /// </summary>
        internal protected CommandMetaGroup CommandMetaGroup { get; set; }

        /// <summary>
        /// 由于本类的职责有两个：生成控件、附加到环境中。所以此API预留给此需要生成控件的场景。
        /// </summary>
        /// <returns></returns>
        public FrameworkElement CreateControl()
        {
            return this.CreateControlCore();
        }

        /// <summary>
        /// 使用动态算法将控件添加到上下文对象中。
        /// </summary>
        /// <param name="control"></param>
        internal virtual void CreateControlToContext()
        {
            var control = this.CreateControlCore();
            if (control == null) return;

            this.AttachToContextCore(control);
        }

        /// <summary>
        /// 子类完成此方法实现具体的生成控件方法。
        /// 
        /// 可以返回 null，不生成任何控件：
        /// 原因是子类有可能用当前环境中某一个已经生成好的控件，也有可能是子类自己把生成的控件附加到环境中了。
        /// </summary>
        /// <returns></returns>
        protected abstract FrameworkElement CreateControlCore();

        /// <summary>
        /// 子类完成此方法实现具体的生成控件方法。
        /// </summary>
        /// <param name="control"></param>
        protected virtual void AttachToContextCore(FrameworkElement control)
        {
            this.AttachToContainer(control);
        }

        #region 辅助方法

        /// <summary>
        /// 添加到右键菜单
        /// </summary>
        /// <param name="control"></param>
        protected virtual void AttachToContextMenu(FrameworkElement control)
        {
            this.Context.ContextMenuItems.Add(control);
        }

        /// <summary>
        /// 简单的添加到Toolbar中
        /// </summary>
        /// <param name="control"></param>
        protected virtual void AttachToContainer(FrameworkElement control)
        {
            this.Context.ContainerItems.Add(control);
        }

        /// <summary>
        /// 为运行时的Command生成一个按钮
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected Button CreateButton(ClientCommand command)
        {
            var btn = new Button();
            btn.CommandParameter = this.Context.CommandArg;
            ButtonCommand.SetCommand(btn, command.UICommand);

            return btn;
        }

        /// <summary>
        /// 为运行时的Command生成一个菜单项
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected MenuItem CreateMenuItem(ClientCommand command)
        {
            MenuItem menuItem = new MenuItem();
            menuItem.CommandParameter = this.Context.CommandArg;
            MenuItemCommand.SetCommand(menuItem, command.UICommand);

            return menuItem;
        }

        private const string TextBox = "GroupGenerator_TextBox";

        /// <summary>
        /// 获取文件框输出值。
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static string GetTextBoxParameter(ClientCommand command)
        {
            return command.GetPropertyOrDefault<string>(TextBox);
        }

        /// <summary>
        /// 设置文件框输入值。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="value"></param>
        public static void SetTextBoxParameter(ClientCommand command, string value)
        {
            command.SetExtendedProperty(TextBox, value);
        }

        /// <summary>
        /// 为一个运行时的 Command 生成 TextBox 控件。
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected static TextBox CreateTextBox(ClientCommand command)
        {
            var textBox = new TipTextBox()
            {
                Width = 150,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(2),
                EmptyValue = command.Label,
                ToolTip = command.Meta.ToolTip.Translate()
            };

            //当TextBox的值改变时，通知命令进行新的输入值
            textBox.TextChanged += (o, e) =>
            {
                var txt = textBox.Text;
                if (txt == textBox.EmptyValue) { txt = string.Empty; }
                SetTextBoxParameter(command, txt);
            };

            //支持UI Test
            AutomationProperties.SetName(textBox, command.Label);

            return textBox;
        }

        #endregion
    }
}