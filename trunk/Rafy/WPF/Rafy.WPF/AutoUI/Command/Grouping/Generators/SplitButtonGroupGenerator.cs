/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101124
 * 说明：为一组命令生成一个下拉的按钮
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
using System.Windows;
using System.Windows.Controls;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF.Controls;

namespace Rafy.WPF.Command.UI
{
    /// <summary>
    /// 为一组命令生成一个下拉的按钮
    /// </summary>
    public class SplitButtonGroupGenerator : GroupGenerator
    {
        protected override FrameworkElement CreateControlCore()
        {
            FrameworkElement generatedResult = null;

            var groupName = this.CommandMetaGroup.Name.Translate();

            //尝试查找已经生成的该组的按钮
            var splitButton = this.Context.ContainerItems.OfType<SplitButton>()
                .FirstOrDefault(btnGroup => btnGroup.Content.Equals(groupName));

            //还没有生成，则创建一个新的。
            if (splitButton == null)
            {
                splitButton = new SplitButton()
                {
                    Mode = SplitButtonMode.Dropdown,
                    Margin = new Thickness(2),
                    Content = groupName,
                };

                //只返回刚生成的 SplitButton
                generatedResult = splitButton;
            }

            this.GenerateButtons(splitButton.Items);

            return generatedResult;
        }

        private void GenerateButtons(ItemCollection items)
        {
            //每个Command都生成一个具体的按钮
            foreach (var command in this.CommandMetaGroup.Commands)
            {
                var group = new CommandMetaGroup(command.Name);
                group.AddCommand(command);

                var menuGenerator = new MenuItemGenerator
                {
                    CommandMetaGroup = group,
                    Context = this.Context
                };
                var control = menuGenerator.CreateControl();
                items.Add(control);
            }
        }

        /// <summary>
        /// 始终在把控件生成在第一的位置。
        /// </summary>
        /// <param name="control"></param>
        protected override void AttachToContextCore(FrameworkElement control)
        {
            this.Context.ContainerItems.Insert(0, control);
        }
    }
}