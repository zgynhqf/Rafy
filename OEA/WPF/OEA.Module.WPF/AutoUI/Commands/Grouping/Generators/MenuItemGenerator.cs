/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101124
 * 说明：单个命令的菜单项生成器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101124
 * 
*******************************************************/

using Itenso.Windows.Input;
using System.Windows;

namespace OEA.Module.WPF.CommandAutoUI
{
    /// <summary>
    /// 单个命令的菜单项生成器
    /// </summary>
    public class MenuItemGenerator : ItemGenerator
    {
        public MenuItemGenerator(CommandGroup group, CommandAutoUIContext context) : base(group, context) { }

        protected override ItemControlResult CreateItemControl()
        {
            var cmd = CommandRepository.NewCommand(this.CommandItem);
            var element = this.CreateAMenuItem(cmd);
            return new ItemControlResult(element, cmd);
        }

        protected override void AttachToContextCore(FrameworkElement control)
        {
            base.AttachToContextMenu(control);
        }
    }
}