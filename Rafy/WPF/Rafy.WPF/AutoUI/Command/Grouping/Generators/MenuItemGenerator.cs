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

using Rafy.WPF.Command;
using System.Windows;

namespace Rafy.WPF.Command.UI
{
    /// <summary>
    /// 单个命令的菜单项生成器
    /// </summary>
    public class MenuItemGenerator : ItemGenerator
    {
        protected override FrameworkElement CreateCommandUI(ClientCommand command)
        {
            return this.CreateMenuItem(command);
        }

        protected override void AttachToContextCore(FrameworkElement control)
        {
            base.AttachToContextMenu(control);
        }
    }
}